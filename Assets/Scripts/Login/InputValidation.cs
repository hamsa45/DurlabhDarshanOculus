using System.Text.RegularExpressions;
public class InputValidation : I_InputValidation
{
    Regex emailRegex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", RegexOptions.IgnoreCase);
    public bool validateEmail(string email)
    {
        return emailRegex.IsMatch(email);
    }
    public bool validatePhone(string phoneNumber)
    {
        return phoneNumber.Length >= 8 && isDigit(phoneNumber);

        bool isDigit(string number)
        {
            foreach (char c in number)
            {
                if (c < '0' || c > '9')
                    return false;
            }
            return true;
        }
    }

}
