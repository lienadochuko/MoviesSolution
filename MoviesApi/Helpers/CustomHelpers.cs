using MoviesApi.Exceptions;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;


namespace MoviesApi.Helpers
{
    public static partial class CustomHelpers
    {
        private static readonly Random random = new();
        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Digits = "0123456789";
        private const string SpecialCharacters = "!@#$%^&*()_+-=[]{}|;:'\",.<>?/`~";

		/// <summary>
		/// Generates a cryptographically secure OTP token along with an expiration time.
		/// </summary>
		/// <param name="length">The length of the OTP token (default is 6).</param>
		/// <param name="expiryInMinutes">The OTP expiration time in minutes (default is 5 minutes).</param>
		/// <returns>A tuple containing the OTP token and its expiration DateTime.</returns>
		public static string GenerateOtpToken(int length = 6)
		{
			if (length <= 0)
			{
				throw new ArgumentException("OTP length must be greater than 0");
			}

			// Maximum value based on the length (e.g., 6 digits = 999,999)
			var maxValue = (int)Math.Pow(10, length) - 1;

			// Use cryptographically secure random number generator
			using var rng = RandomNumberGenerator.Create();
			var bytes = new byte[sizeof(int)];
			rng.GetBytes(bytes);
			var randomValue = Math.Abs(BitConverter.ToInt32(bytes, 0));

			// Ensure the number is within the desired range
			var otpValue = randomValue % (maxValue + 1);

			// Format the OTP to the specified length (padded with leading zeros)
			string otp = otpValue.ToString().PadLeft(length, '0');

			return otp;
		}

		/// <summary>
		/// Validates if the OTP is expired based on the expiration time.
		/// </summary>
		/// <param name="expirationTime">The expiration time of the OTP.</param>
		/// <returns>True if the OTP is still valid, false if it has expired.</returns>
		public static bool IsOtpExpired(DateTime expirationTime)
		{
			return DateTime.UtcNow > expirationTime;
		}

		// Static method for cleansing narration
		public static string RemoveSpecialCharacters(string text)
        {
            // Replace non-alphanumeric characters with a space
            string alphaNumericText = SpecialCharactersRegex().Replace(text, " ");

            // Replace consecutive spaces with a single space
            string cleansedNarration = DoubleSpaceRegex().Replace(alphaNumericText, " ");

            // Trim leading and trailing spaces
            return cleansedNarration.Trim();
        }

        public static bool ArePasswordsTooSimilar(string oldPassword, string newPassword)
        {
            // Check for exact matches
            if (string.Equals(oldPassword, newPassword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Check for partial matches (case-insensitive)
            if (newPassword.Contains(oldPassword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Check for reversed matches (case-insensitive)
            string reversedOldPassword = new(oldPassword.Reverse().ToArray());
            if (newPassword.Contains(reversedOldPassword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Check for common variations (case-insensitive)
            // Replace characters with common substitutions
            string modifiedOldPassword = oldPassword.Replace("o", "0")
                                                    .Replace("i", "1")
                                                    .Replace("e", "3")
                                                    .Replace("a", "@");
            if (newPassword.Contains(modifiedOldPassword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Check for significant character overlap (75% or more)
            int overlapCount = 0;
            for (int i = 0; i < Math.Min(oldPassword.Length, newPassword.Length); i++)
            {
                if (char.ToLowerInvariant(oldPassword[i]) == char.ToLowerInvariant(newPassword[i]))
                {
                    overlapCount++;
                }
            }
            double overlapPercentage = (double)overlapCount / Math.Min(oldPassword.Length, newPassword.Length) * 100;
            if (overlapPercentage >= 40)
            {
                return true;
            }

            // If none of the checks match, consider the passwords different enough
            return false;
        }

        public static string GetLastLoginText(DateTime? lastLogin)
        {
            DateTime currentDate = DateTime.Now.Date;
            DateTime lastLoginDate = lastLogin.GetValueOrDefault().Date;

            TimeSpan timeDifference = currentDate - lastLoginDate;
            int daysDifference = timeDifference.Days;

            if (daysDifference == 0)
            {
                return "Today";
            }
            else if (daysDifference == 1)
            {
                return "Yesterday";
            }
            else if (daysDifference < 30)
            {
                return $"{daysDifference} days ago";
            }
            else
            {
                int months = daysDifference / 30;
                int remainingDays = daysDifference % 30;

                if (remainingDays == 0)
                {
                    return $"{months} months ago";
                }
                else
                {
                    return $"{months} months and {remainingDays} days ago";
                }
            }
        }

        public static string GenerateRandomNumber(int length)
        {
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] randomBytes = new byte[length];
            rng.GetBytes(randomBytes);
            StringBuilder sb = new(length);

            foreach (byte b in randomBytes)
            {
                sb.Append(b % 10); // Ensure the value is between 0 and 9
            }

            return sb.ToString();
        }

        public static string RemoveNonDigits(string input)
        {
            return DigitsOnlyRegex().Replace(input, "");
        }

        public static string GenerateReferenceNumber()
        {
            var dateTimePart = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] randomBytes = new byte[4];
            rng.GetBytes(randomBytes);

            // Use BitConverter to convert the random bytes to an integer
            int randomNumberPart = BitConverter.ToInt32(randomBytes, 0);

            return $"{dateTimePart}{Math.Abs(randomNumberPart)}";
        }

        public static string GenerateUniqueReference()
        {
            const string start = "11";
            const string end = "55";
            var datePart = DateTime.Now.ToString("ddMMyyyy");
            var randomDigits = GenerateRandomNumber(8);
            return start + datePart + randomDigits + end;
        }

        public static bool ValidateUniqueReference(string reference)
        {
            return reference.Length == 20 && reference.StartsWith("11") && reference.EndsWith("55");
        }

        public static string GenerateRandomPassword(int length = 8)
        {
            if (length < 8)
                throw new ArgumentException("Password length must be at least 8 characters.");

            // Ensure the password has at least one of each required character type
            var password = new StringBuilder();
            password.Append(Lowercase[random.Next(Lowercase.Length)]);
            password.Append(Uppercase[random.Next(Uppercase.Length)]);
            password.Append(Digits[random.Next(Digits.Length)]);
            password.Append(SpecialCharacters[random.Next(SpecialCharacters.Length)]);

            // Fill the rest of the password length with a random selection of the character types
            const string allCharacters = Lowercase + Uppercase + Digits + SpecialCharacters;
            for (int i = 4; i < length; i++)
            {
                password.Append(allCharacters[random.Next(allCharacters.Length)]);
            }

            // Shuffle the resulting password to ensure randomness
            return new string(password.ToString().OrderBy(_ => random.Next()).ToArray());
        }

        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[randomBytes[i] % chars.Length]);
            }

            return sb.ToString();
        }

        public static byte[] GenerateAESKey(string password, byte[] salt, int keySize)
        {
            using var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            return rfc2898DeriveBytes.GetBytes(keySize / 8);
        }

        public static string GetConnectionString(IConfiguration configuration, string databaseName)
        {
            return configuration.GetConnectionString($"{databaseName}") ?? string.Empty;
        }

        public static Tuple<string, string> GetEncryptionDetails(IConfiguration configuration)
        {
            return new Tuple<string, string>(configuration["Unlock:Key"] ?? "", configuration["Unlock:Salt"] ?? "");
        }

        public static string EncryptString(string plainText, string saltTxt, string keyTxt)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new CustomException("The string to decrypt is invalid!");
            byte[] salt = Encoding.UTF8.GetBytes(saltTxt);
            byte[] encryptionKey = GenerateAESKey(keyTxt, salt, 256); // 256-bit key

            using Aes aesAlg = Aes.Create();
            aesAlg.Key = encryptionKey;
            aesAlg.GenerateIV(); // Generate a new IV for each encryption

            using var encryptor = aesAlg.CreateEncryptor();

            byte[] iv = aesAlg.IV; // Get the IV

            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

            // Convert the plainText to bytes and write to the CryptoStream
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
            cryptoStream.FlushFinalBlock();

            // Combine IV and encrypted data
            byte[] ivAndCiphertext = new byte[iv.Length + memoryStream.Length];
            iv.CopyTo(ivAndCiphertext, 0);
            memoryStream.ToArray().CopyTo(ivAndCiphertext, iv.Length);

            return Convert.ToBase64String(ivAndCiphertext);
        }

        public static string DecryptString(string cipherText, string saltTxt, string keyTxt)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new CustomException("The string to decrypt is invalid!");
            byte[] salt = Encoding.UTF8.GetBytes(saltTxt);
            byte[] encryptionKey = GenerateAESKey(keyTxt, salt, 256); // 256-bit key

            byte[] ivAndCiphertext = Convert.FromBase64String(cipherText);
            byte[] iv = new byte[16]; // IV size is 16 bytes

            // Extract IV
            Buffer.BlockCopy(ivAndCiphertext, 0, iv, 0, iv.Length);

            // Extract ciphertext
            byte[] cipherBytes = new byte[ivAndCiphertext.Length - iv.Length];
            Buffer.BlockCopy(ivAndCiphertext, iv.Length, cipherBytes, 0, cipherBytes.Length);

            using Aes aesAlg = Aes.Create();
            aesAlg.Key = encryptionKey;
            aesAlg.IV = iv; // Set the IV

            using var decryptor = aesAlg.CreateDecryptor();

            using var memoryStream = new MemoryStream(cipherBytes);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

            using var reader = new StreamReader(cryptoStream);

            // Read the decrypted data and return as string
            return reader.ReadToEnd();
        }

        public static string RemoveExtraSpacesAndCommas(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            // Remove commas, extra spaces, and specific whitespace characters
            text = text.Replace(",", "").Replace("\r", "").Replace("\t", "").Replace("\n", "");

            // Remove extra spaces between words
            text = MyRegex().Replace(text, " ");

            // Trim the resulting string
            return text.Trim();
        }

        public static string RemoveExtraSpacesAndCommasAndSpecialCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            // Remove commas, extra spaces, and specific whitespace characters
            text = text.Replace(",", "").Replace("\r", "").Replace("\t", "").Replace("\n", "");

            // Remove extra spaces between words
            text = MyRegex().Replace(text, " ");

            // Remove special characters, keeping only alphabets and numbers
            text = SpecialCharactersRegex().Replace(text, "");

            // Trim the resulting string
            return text.Trim();
        }

        public static string GetSafeString(SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? string.Empty : reader.GetString(index);
        }

		public static Guid GetSafeGuid(SqlDataReader reader, int index)
		{
			return reader.IsDBNull(index) ? Guid.Empty : reader.GetGuid(index);
		}

		public static string GetSafeDecimalString(SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? string.Empty : SplitNumberWithComma(reader.GetDecimal(index).ToString());
        }

        public static string GetSafeDecimalString2(SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? string.Empty : reader.GetDecimal(index).ToString();
        }

        public static decimal GetSafeDecimal(SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? 0 : reader.GetDecimal(index);
        }
		public static string GetSafeDoubleString(SqlDataReader reader, int index)
		{
			return reader.IsDBNull(index) ? string.Empty : SplitNumberWithComma(reader.GetDouble(index).ToString());
		}

		public static double GetSafeDouble(SqlDataReader reader, int index)
		{
			return reader.IsDBNull(index) ? 0 : reader.GetDouble(index);
		}

		public static string GetSafeInt32String(SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? string.Empty : reader.GetInt32(index).ToString();
        }

        public static string GetSafeInt16String(SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? string.Empty : reader.GetInt16(index).ToString();
        }

        public static int GetSafeInt16(SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? 0 : reader.GetInt16(index);
        }
		public static byte GetSafeByte(SqlDataReader reader, int index)
		{
			return reader.IsDBNull(index) ? (byte)0 : reader.GetByte(index);
		}


		public static string GetSafeInt64String(SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? string.Empty : reader.GetInt64(index).ToString();
        }

        public static int GetSafeInt32(SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? 0 : reader.GetInt32(index);
        }

		public static long GetSafeInt64(SqlDataReader reader, int index)
		{
			return reader.IsDBNull(index) ? 0 : reader.GetInt64(index);
		}
  //      public static long GetSafeLong(SqlDataReader reader, int index)
		//{
		//	return reader.IsDBNull(index) ? 0 : reader.GetInt64(index);
		//}

		public static string GetSafeBooleanString(SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? string.Empty : reader.GetBoolean(index).ToString().ToUpper();
        }

		public static bool GetSafeBoolean(SqlDataReader reader, int index)
		{
			return !reader.IsDBNull(index) && reader.GetBoolean(index);
		}

		public static string GetSafeDateTimeString(SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? string.Empty : reader.GetDateTime(index).ToString("dd-MM-yyyy");
        }

        public static string GetSafeDateTimeString2(SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? string.Empty : reader.GetDateTime(index).ToString("dd/MM/yyyy");
        }

		public static DateTime? GetDateTime(SqlDataReader reader, int index)
		{
			return reader.IsDBNull(index) ? null : reader.GetDateTime(index);
		}

		public static string SplitNumberWithComma(string number)
        {
            if (!string.IsNullOrEmpty(number))
            {
                if (number.Contains('.'))
                {
                    string withoutDecimals = number[..number.IndexOf('.')];
                    string withDecimals = number[number.IndexOf('.')..];

                    // Convert the part without decimals to a long and format with commas
                    withoutDecimals = Convert.ToInt64(withoutDecimals).ToString("#,##0");

                    // Convert the part with decimals to a double, round to two decimal places, and format
                    double decimalPart = Convert.ToDouble("0" + withDecimals); // Ensure "0" is added before the decimal part
                    string roundedDecimalPart = Math.Round(decimalPart, 2).ToString(".00")[1..];

                    return withoutDecimals + "." + roundedDecimalPart;
                }
                else
                {
                    return Convert.ToInt64(number).ToString("#,##0");
                }
            }

            return string.Empty;
        }

        public static decimal ParseNumberWithComma(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                // Remove commas from the input string
                string numberWithoutComma = value.Replace(",", "");

                // Parse the string into a decimal
                if (decimal.TryParse(numberWithoutComma, out decimal result))
                {
                    return result;
                }
            }

            // Default value or throw an exception, depending on your requirement
            return 0;
        }

        //Helper method for checking similarities between two strings, returns a number between 0 and 1
        public static int LevenshteinDistance(string str1, string str2)
        {
            int[,] dp = new int[str1.Length + 1, str2.Length + 1];

            for (int i = 0; i <= str1.Length; i++)
            {
                for (int j = 0; j <= str2.Length; j++)
                {
                    if (i == 0)
                    {
                        dp[i, j] = j;
                    }
                    else if (j == 0)
                    {
                        dp[i, j] = i;
                    }
                    else
                    {
                        dp[i, j] = Min(dp[i - 1, j - 1] + (str1[i - 1] == str2[j - 1] ? 0 : 1),
                                                               dp[i - 1, j] + 1,
                                                               dp[i, j - 1] + 1);
                    }
                }
            }

            return dp[str1.Length, str2.Length];
        }

        public static int Min(int a, int b, int c)
        {
            return Math.Min(Math.Min(a, b), c);
        }

        [GeneratedRegex("\\s+")]
        private static partial Regex MyRegex();
        [GeneratedRegex(@"[^a-zA-Z0-9]+")]
        private static partial Regex SpecialCharactersRegex();
        [GeneratedRegex(@"\s+")]
        private static partial Regex DoubleSpaceRegex();
        [GeneratedRegex(@"\D")]
        private static partial Regex DigitsOnlyRegex();
    }
}