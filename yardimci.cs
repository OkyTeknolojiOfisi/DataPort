using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataPort
{
    /// <summary>
    /// Yardımcı metodları barındıran sınıf
    /// </summary>
    class yardimci
    {
        /// <summary>
        /// Gönderilen telefon numarasının kayıt için uygunluğu kontrol edilir.
        /// </summary>
        /// <param name="telefon">Kontrol yapılacak telefon numarası</param>
        /// <returns>True/False</returns>
        public static bool TelefonFormat(string telefon)
        {
            string[] gsm = { "501", "505", "506", "507", "551", "552", "553", "554", "555", "559", "530", "531", "532", "533", "534", "535", "536", "537", "538", "539", "540", "541", "542", "543", "544", "545", "546", "547", "548", "549" };

            if (telefon.Length != 10) { return false; }

            if (!gsm.Contains(telefon.Substring(0, 3))) { return false; }

            return true;
        }
    }
}
