using System.Text.RegularExpressions;

namespace BackEnd.Application.Utils
{
    public static class EmployeeUtils
    {
        /// <summary>전화번호에서 하이픈을 제거합니다.</summary>
        public static string RemoveTelHyphen(string tel)
        {
            return tel.Replace("-", string.Empty);
        }

        /// <summary>숫자 이외의 모든 문자를 제거합니다.</summary>
        public static string RemoveNonNumeric(string value)
        {
            return Regex.Replace(value, @"[^\d]", string.Empty);
        }

        /// <summary>이메일 형식을 정규식으로 검증합니다.</summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
    }
}
