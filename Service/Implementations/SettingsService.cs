using AutoMapper;
using Core.Data;
using Core.Entities;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Extensions;
using Service.Helpers;
using Service.Interfaces;
using Service.Models;
using Service.Models.ViewModels;
using Service.StringKeys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Service.Implementation
{
    public class SettingsService : ISetting
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly HttpContext _httpContext;

        public SettingsService(ApplicationDbContext context, IHttpContextAccessor contextAccessor, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
            _httpContext = contextAccessor.HttpContext;
        }

        private string UserId => _httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private async Task<Result<ApplicationUser>> GetCurrentUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(UserId))
                return Result.Failure<ApplicationUser>("Unauthorized");

            return Result.Success(await _context.Users
                .Include(c => c.State)
                .AsNoTracking().FirstOrDefaultAsync(c => c.Id == UserId));
        }

        public async Task<AppResult<string>> Save(SettingsViewModel model)
        {
            var setting = await _context.Settings.FirstOrDefaultAsync();

            setting.HasDateReportedValidation = model.HasDateReportedValidation;
            setting.IsGBVQuestionsEnabled = model.IsGBVQuestionsEnabled;
            setting.AllowPrevMonthCases = model.AllowPrevMonthCases;
            setting.ModifiedAt = DateTime.Now;

            _context.Update(setting);

            await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = "Settings Saved Successfully",
                Message = "Settings Saved Successfully"
            };
        }

        public async Task<AppResult<SettingsViewModel>> GetSettings()
        {
            var settings = await _context.Settings.Select(c => new SettingsViewModel
            {
                HasDateReportedValidation = c.HasDateReportedValidation,
                IsGBVQuestionsEnabled = c.IsGBVQuestionsEnabled,
                AllowPrevMonthCases = c.AllowPrevMonthCases
            }).FirstOrDefaultAsync();

            return new AppResult<SettingsViewModel>
            {
                Data = settings,
                Message = "Successful"
            };
        }
        public async Task<AppResult<List<StatesVM>>> GetAllStates()
        {
            var result = new AppResult<List<StatesVM>>();
            var states = _context.States.AsNoTracking();
            var data = await states.Select(x => new StatesVM
            {
                Code = x.Code,
                Name = x.Name,
                Id = x.Id,
                Key = x.Key,
                //LocalGovernmentAreas = x.LocalGovernmentAreas.Select(x => new LocalGovernmentsVM
                //{
                //    Id = x.Id,
                //    //Key = x.Key,
                //    Name = x.Name,
                //    Wards = x.Wards.Select(x => new WardsVM
                //    {
                //        Id = x.Id,
                //        Name = x.Name,
                //        //Key = x.Key,
                //    }).ToList()
                //}).ToList()
            }).ToListAsync();
            result.Data = data;
            result.Message = "Successful";
            result.StatusCode = StatusCodes.Status200OK;

            return result;
        }

        public async Task<AppResult<StatesVM>> GetAState(int id)
        {
            var result = new AppResult<StatesVM>();
            var states = await _context.States.AsNoTracking().Include(x => x.LocalGovernmentAreas).ThenInclude(x => x.Wards).FirstOrDefaultAsync(x => x.Id == id);
            var data = new StatesVM
            {
                Code = states.Code,
                Name = states.Name,
                Id = states.Id,
                Key = states.Key,
                LocalGovernmentAreas = states.LocalGovernmentAreas.Select(x => new LocalGovernmentsVM
                {
                    Id = x.Id,
                    Key = x.Key,
                    Name = x.Name,
                    Wards = x.Wards.Select(x => new WardsVM
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Key = x.Key,
                    }).ToList()
                }).ToList()
            };
            result.Data = data;
            result.Message = "Successful";
            result.StatusCode = StatusCodes.Status200OK;

            return result;
        }
        public async Task<Result<int>> UploadFile(FileUploadRequest request, FileUploadCategory category)
        {
            var currentUserResult = await GetCurrentUser(UserId);
            if (currentUserResult.IsFailure || currentUserResult.Value is null)
                return Result.Failure<int>(currentUserResult.Error);

            // setup icon bytes
            byte[] fileBytes;
            await using (var ms = new MemoryStream())
            {
                if (request.File is null)
                {
                    fileBytes = null;
                }
                else
                {
                    await request.File.CopyToAsync(ms);
                    fileBytes = ms.ToArray();
                }
            }

            var file = new FileUpload(
                request.Name,
                new FileUploadMetaData(request.File.FileName, request.File.ContentType, request.File.Length), //todo: add data
                request.Description,
                category,
                null,
                fileBytes,
                false,
                currentUserResult.Value.Id);

            await _context.FilesUploads.AddAsync(file);

            var status = await _context.SaveChangesAsync() > 0;

            return status ? Result.Success(file.Id) : Result.Failure<int>("Failed to save");
        }

        public async Task<Result> ChangeFileInfo(UpdateFileInfoRequest request)
        {
            var file = await _context.FilesUploads.FirstOrDefaultAsync(f => f.Id == request.Id);

            if (file is null) return Result.Failure("File not found");

            file.ChangeInfo(request.Name, request.Description);

            _context.Entry(file).State = EntityState.Modified;

            var status = await _context.SaveChangesAsync() > 0;

            return status ? Result.Success("Success") : Result.Failure("Failed to save");
        }

        public async Task<Result<PaginatedList<FileUploadDto>>> GetAllFiles(FileUploadCategory? type, int pageIndex, int pageSize)
        {
            var files = _context.FilesUploads.Include(f => f.CreatedBy).AsQueryable();

            if (type.HasValue)
            {
                if (type == FileUploadCategory.Template)
                {
                    var user = await GetCurrentUser(UserId);
                    if (user.Value == null)
                        files = null;
                    else
                    {
                        files = files.Where(x => x.UploadCategory == FileUploadCategory.Template);
                    }

                }

                else
                {
                    files = files.Where(f => type.HasValue && f.UploadCategory == type);
                }

            }
            else
            {
                files = files.Where(x => x.UploadCategory != FileUploadCategory.Template);
            }

            var fileDtos = _mapper.Map<List<FileUploadDto>>(await files.ToListAsync());

            return Result.Success(fileDtos.Page(pageIndex, pageSize));
        }

        public async Task<Result<FileUploadDto>> GetFileById(int id)
        {
            var file = await _context.FilesUploads
                .Include(f => f.CreatedBy)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (file is null) return Result.Failure<FileUploadDto>("File not found");

            var fileDto = _mapper.Map<FileUploadDto>(file);

            return Result.Success(fileDto);
        }

        public async Task<Result> DeleteFile(int id)
        {
            var file = await _context.FilesUploads
                .FirstOrDefaultAsync(f => f.Id == id);

            if (file is null) return Result.Failure<FileUploadDto>("File not found");

            _context.Remove(file);

            var status = await _context.SaveChangesAsync() > 0;

            return status ? Result.Success() : Result.Failure("Failed to delete");
        }
    }
}