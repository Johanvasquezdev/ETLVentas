using System.Collections.Generic;

namespace ETLVentas.Data.Validators
{
    public class DuplicateValidator
    {
        public bool IsDuplicate<T>(T item, HashSet<T> uniqueSet)
        {
            return !uniqueSet.Add(item);
        }
    }
}
