namespace ETLVentas.Data.Validators
{
    public class RequiredFieldsValidator
    {
        public bool IsValidString(string input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }
    }
}
