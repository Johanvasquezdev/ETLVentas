namespace ETLVentas.Data.Validators
{
    public class TypeValidator
    {
        public bool IsValidDecimal(string input, out decimal result)
        {
            return decimal.TryParse(input, out result);
        }

        public bool IsValidInt(string input, out int result)
        {
            return int.TryParse(input, out result);
        }

        public bool IsValidDateTime(string input, out System.DateTime result)
        {
            return System.DateTime.TryParse(input, out result);
        }
    }
}
