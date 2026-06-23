using AutoMapper;
using Core.Data;
using Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Extensions;
using Service.Helpers;
using Service.Interfaces;
using Service.Models;
using Service.StringKeys;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Service.Implementations
{
    public class ComplaintFormService : IBusinessService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;

        public ComplaintFormService(ApplicationDbContext context, IHttpContextAccessor accessor, IMapper mapper)
        {
            _context = context;
            _accessor = accessor;
            _mapper = mapper;
        }

        private string UserId => _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public async Task<(bool status, string message, int id)> CreateComplaintForm(CreateComplaintFormDto request)
        {
            var currentUserExists = await _context.Users
                .AsNoTracking().Where(c => c.Id == UserId)
                .Select(u => new { StateCode = u.State.Code, OrganisationCode = u.Organisation.Code, u.StateId, u.Id })
                .FirstOrDefaultAsync();

            if (currentUserExists is null) return (false, "Logged in user not found", default);

            var attachments = new List<FileUpload>();
            if (request.Attachements != null)
            {
                foreach (var attachment in request.Attachements)
                {
                    // setup icon bytes
                    byte[] fileBytes;
                    await using (var ms = new MemoryStream())
                    {
                        await attachment.CopyToAsync(ms);
                        fileBytes = ms.ToArray();
                    }

                    attachments.Add(new FileUpload(
                          attachment.FileName,
                          new FileUploadMetaData(attachment.FileName, attachment.ContentType, attachment.Length), //todo: add data
                          request.Subject,
                          FileUploadCategory.ComplaintForm,
                          null,
                          fileBytes,
                          false,
                          UserId));
                }
            }

            var codeResult = await GenerateComplaintCode(currentUserExists.StateCode, currentUserExists.OrganisationCode);

            var form = new ComplaintForm(
                UserId,
                request.ComplaintType,
                request.Subject,
                request.Body,
                attachments,
                codeResult.complaintCode,
                codeResult.serial);

            await _context.ComplaintForms.AddAsync(form);

            var status = await _context.SaveChangesAsync() > 0;

            return status ? (status, "Success", form.Id) : (status, "Failed to save", default);
        }

        public async Task<(bool status, string message, PaginatedList<ComplaintFormDto> data)> GetAll(GetAllCompaintFormsRequest request)
        {
            var currentUserExists = await _context.Users
                .AsNoTracking().FirstOrDefaultAsync(c => c.Id == UserId);

            if (currentUserExists == null) return (false, "Logged in user not found", default);

            var query = _context.ComplaintForms
                .OrderByDescending(c => c.CreatedAt)
                .AsNoTracking();

            if (currentUserExists.Type != RoleKeys.Administrator)
            {
                query = query.Where(c => c.User.OrganisationId == currentUserExists.OrganisationId);
            }

            if (request.ComplaintType.HasValue)
            {
                query = query.Where(c => c.ComplaintType == request.ComplaintType.Value);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(c => c.Status == request.Status.Value);
            }

            var dtos = await query.PageProjectAsync<ComplaintForm, ComplaintFormDto>(_mapper, request.PageIndex, request.PageSize);

            return (true, "Success", dtos);
        }

        public async Task<(bool status, string message, ComplaintFormDto data)> GetById(int id)
        {
            var form = await _context.ComplaintForms.AsNoTracking()
                .Include(f => f.Attachements)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (form == null) return (false, "Complaint form not found", default);

            var dto = _mapper.Map<ComplaintFormDto>(form);

            return (true, "Success", dto);
        }

        public async Task<(bool status, string message)> ChangeFormStatus(ChangeComplaintFormStatusRequest request)
        {
            var currentUserExists = await _context.Users
                    .AsNoTracking().FirstOrDefaultAsync(c => c.Id == UserId);

            if (currentUserExists == null) return (false, "Logged in user not found");

            var form = await _context.ComplaintForms
           .FirstOrDefaultAsync(f => f.Id == request.Id);

            if (form == null) return (false, "Complaint form not found");

            if (request.Status == ComplaintStatus.Resolved)
            {
                form.ResolveComplaint(currentUserExists.Id, DateTime.UtcNow);
            }

            await _context.SaveChangesAsync();

            return (true, "Success");
        }

        private async Task<(string complaintCode, int serial)> GenerateComplaintCode(string stateCode, string organisationCode)
        {
            var todaysDate = DateTime.Now;
            var lastCaseSerialNumber = await _context.ComplaintForms
                .Where(c => c.CreatedAt.Month == todaysDate.Month && todaysDate.Year == c.CreatedAt.Year && c.User.Organisation.Code == organisationCode)
                .OrderBy(c => c.SerialNumber).Select(c => c.SerialNumber).LastOrDefaultAsync();

            var serialno = 0;
            if (lastCaseSerialNumber == 0)
            {
                serialno = 1;
            }
            else
            {
                serialno = lastCaseSerialNumber + 1;
            }

            var monthName = todaysDate.ToString("MMM", CultureInfo.InvariantCulture);
            return ComplaintCodeAndSerialNumber(stateCode, organisationCode, serialno, todaysDate.Year.ToString(), monthName);
        }

        /// <summary>
        /// SER/CHU/21/NOV/001
        /// </summary>
        /// <param name="stateCode"></param>
        /// <param name="organisationCode"></param>
        /// <param name="serialNo"></param>
        /// <returns></returns>
        private static (string serviceProvisionCode, int serial) ComplaintCodeAndSerialNumber(string stateCode, string organisationCode, int serialNo, string year, string month) =>
            ($"COMP/{stateCode.ToUpper()}/{organisationCode}/{year.Substring(year.Length - 2)}/{month.ToUpper()}/{serialNo:0000}", serialNo);
    }
}