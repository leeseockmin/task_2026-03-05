using BackEnd.Application.Utils;
using Xunit;

namespace BackEnd.Tests.Utils
{
    /// <summary>
    /// EmployeeUtils 정적 유틸 메서드 단위 테스트
    /// </summary>
    public class EmployeeUtilsTests
    {
        // =============================================
        // RemoveTelHyphen 테스트
        // =============================================

        /// <summary>
        /// [성공] Tel에 하이픈이 포함된 경우 — 하이픈이 모두 제거된 번호를 반환한다.
        /// </summary>
        [Fact]
        public void RemoveTelHyphen_TelWithHyphens_ReturnsHyphensRemoved()
        {
            // Arrange
            var tel = "010-1234-5678";

            // Act
            var result = EmployeeUtils.RemoveTelHyphen(tel);

            // Assert
            Assert.Equal("01012345678", result);
        }

        /// <summary>
        /// [성공] Tel에 하이픈이 없는 경우 — 입력값 그대로를 반환한다.
        /// </summary>
        [Fact]
        public void RemoveTelHyphen_TelWithoutHyphens_ReturnsSameValue()
        {
            // Arrange
            var tel = "01012345678";

            // Act
            var result = EmployeeUtils.RemoveTelHyphen(tel);

            // Assert
            Assert.Equal("01012345678", result);
        }

        /// <summary>
        /// [성공] 빈 문자열 입력 — 빈 문자열을 반환한다.
        /// </summary>
        [Fact]
        public void RemoveTelHyphen_EmptyString_ReturnsEmptyString()
        {
            // Arrange
            var tel = string.Empty;

            // Act
            var result = EmployeeUtils.RemoveTelHyphen(tel);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// [성공] 하이픈으로만 구성된 문자열 — 하이픈이 모두 제거되어 빈 문자열을 반환한다.
        /// </summary>
        [Fact]
        public void RemoveTelHyphen_OnlyHyphens_ReturnsEmptyString()
        {
            // Arrange
            var tel = "---";

            // Act
            var result = EmployeeUtils.RemoveTelHyphen(tel);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// [성공] 국제 전화번호 형식(하이픈 포함) — 하이픈이 제거된 번호를 반환한다.
        /// </summary>
        [Fact]
        public void RemoveTelHyphen_InternationalFormat_ReturnsHyphensRemoved()
        {
            // Arrange
            var tel = "+82-10-1234-5678";

            // Act
            var result = EmployeeUtils.RemoveTelHyphen(tel);

            // Assert
            Assert.Equal("+821012345678", result);
        }

        // =============================================
        // IsValidEmail 테스트
        // =============================================

        /// <summary>
        /// [성공] 올바른 이메일 형식 — true를 반환한다.
        /// </summary>
        [Fact]
        public void IsValidEmail_ValidEmailFormat_ReturnsTrue()
        {
            // Arrange
            var email = "user@example.com";

            // Act
            var result = EmployeeUtils.IsValidEmail(email);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// [성공] 서브도메인이 포함된 이메일 — true를 반환한다.
        /// </summary>
        [Fact]
        public void IsValidEmail_EmailWithSubdomain_ReturnsTrue()
        {
            // Arrange
            var email = "user@mail.example.co.kr";

            // Act
            var result = EmployeeUtils.IsValidEmail(email);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// [성공] 대문자가 포함된 이메일 — 정규식에 IgnoreCase 옵션이 적용되어 있으므로 true를 반환한다.
        /// </summary>
        [Fact]
        public void IsValidEmail_UppercaseEmail_ReturnsTrue()
        {
            // Arrange
            var email = "User@Example.COM";

            // Act
            var result = EmployeeUtils.IsValidEmail(email);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// [실패] @ 기호가 없는 이메일 — false를 반환한다.
        /// </summary>
        [Fact]
        public void IsValidEmail_NoAtSign_ReturnsFalse()
        {
            // Arrange — @ 없음
            var email = "userexample.com";

            // Act
            var result = EmployeeUtils.IsValidEmail(email);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// [실패] 도메인에 점(.)이 없는 이메일 — false를 반환한다.
        /// </summary>
        [Fact]
        public void IsValidEmail_NoDotInDomain_ReturnsFalse()
        {
            // Arrange — 점 없음
            var email = "user@examplecom";

            // Act
            var result = EmployeeUtils.IsValidEmail(email);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// [실패] @ 기호가 중복된 이메일 — false를 반환한다.
        /// </summary>
        [Fact]
        public void IsValidEmail_DoubleAtSign_ReturnsFalse()
        {
            // Arrange — @ 중복
            var email = "user@@example.com";

            // Act
            var result = EmployeeUtils.IsValidEmail(email);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// [실패] 로컬 파트가 없는 이메일(@로 시작) — false를 반환한다.
        /// </summary>
        [Fact]
        public void IsValidEmail_NoLocalPart_ReturnsFalse()
        {
            // Arrange — 로컬 파트 없음
            var email = "@example.com";

            // Act
            var result = EmployeeUtils.IsValidEmail(email);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// [실패] 도메인이 점(.)으로 시작하는 비정상 형식 — false를 반환한다.
        /// </summary>
        [Fact]
        public void IsValidEmail_DomainStartsWithDot_ReturnsFalse()
        {
            // Arrange — 비정상 도메인
            var email = "user@.com";

            // Act
            var result = EmployeeUtils.IsValidEmail(email);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// [실패] null 입력 — 예외 없이 false를 반환한다.
        /// </summary>
        [Fact]
        public void IsValidEmail_Null_ReturnsFalse()
        {
            // Act
            var result = EmployeeUtils.IsValidEmail(null!);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// [실패] 빈 문자열 입력 — false를 반환한다.
        /// </summary>
        [Fact]
        public void IsValidEmail_EmptyString_ReturnsFalse()
        {
            // Act
            var result = EmployeeUtils.IsValidEmail(string.Empty);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// [실패] 공백으로만 이루어진 문자열 — false를 반환한다.
        /// </summary>
        [Fact]
        public void IsValidEmail_WhitespaceOnly_ReturnsFalse()
        {
            // Act
            var result = EmployeeUtils.IsValidEmail("   ");

            // Assert
            Assert.False(result);
        }

        // =============================================
        // RemoveNonNumeric 테스트
        // =============================================

        /// <summary>
        /// [성공] 하이픈이 포함된 경우 — 숫자만 남기고 제거한다.
        /// </summary>
        [Fact]
        public void RemoveNonNumeric_WithHyphens_ReturnsOnlyDigits()
        {
            // Arrange
            var value = "010-2586";

            // Act
            var result = EmployeeUtils.RemoveNonNumeric(value);

            // Assert
            Assert.Equal("0102586", result);
        }

        /// <summary>
        /// [성공] 점(.)이 포함된 경우 — 숫자만 남기고 제거한다.
        /// </summary>
        [Fact]
        public void RemoveNonNumeric_WithDots_ReturnsOnlyDigits()
        {
            // Arrange
            var value = "010.2586";

            // Act
            var result = EmployeeUtils.RemoveNonNumeric(value);

            // Assert
            Assert.Equal("0102586", result);
        }

        /// <summary>
        /// [성공] 다양한 특수문자가 혼합된 경우 — 숫자만 남기고 제거한다.
        /// </summary>
        [Fact]
        public void RemoveNonNumeric_WithMixedSpecialChars_ReturnsOnlyDigits()
        {
            // Arrange
            var value = "+82-10-1234-5678";

            // Act
            var result = EmployeeUtils.RemoveNonNumeric(value);

            // Assert
            Assert.Equal("821012345678", result);
        }

        /// <summary>
        /// [성공] 숫자만 있는 경우 — 입력값 그대로를 반환한다.
        /// </summary>
        [Fact]
        public void RemoveNonNumeric_OnlyDigits_ReturnsSameValue()
        {
            // Arrange
            var value = "0102586";

            // Act
            var result = EmployeeUtils.RemoveNonNumeric(value);

            // Assert
            Assert.Equal("0102586", result);
        }

        /// <summary>
        /// [성공] 숫자가 없는 경우 — 빈 문자열을 반환한다.
        /// </summary>
        [Fact]
        public void RemoveNonNumeric_NoDigits_ReturnsEmptyString()
        {
            // Arrange
            var value = "abc-def";

            // Act
            var result = EmployeeUtils.RemoveNonNumeric(value);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// [성공] 빈 문자열 입력 — 빈 문자열을 반환한다.
        /// </summary>
        [Fact]
        public void RemoveNonNumeric_EmptyString_ReturnsEmptyString()
        {
            // Arrange
            var value = string.Empty;

            // Act
            var result = EmployeeUtils.RemoveNonNumeric(value);

            // Assert
            Assert.Equal(string.Empty, result);
        }
    }
}
