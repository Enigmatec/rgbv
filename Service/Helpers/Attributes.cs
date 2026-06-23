using System.ComponentModel.DataAnnotations;

namespace Service.Helpers
{
    public class ListCannotBeEmptyAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            dynamic d = value;

            return d != null && d.Count > 0;
        }
    }

    public class ValueCannotBeZero : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var item = (int)value;

            return item != 0;
        }
    }

    public class StringCannotBeEmpty : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var item = (string)value;

            return !string.IsNullOrWhiteSpace(item);
        }
    }
}