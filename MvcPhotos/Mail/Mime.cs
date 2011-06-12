using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MvcPhotos.Mail
{
    public static class Mime
    {
        public static readonly string DefaultCharset = "ISO-2022-JP";
        public static readonly string DefaultEncoding = "7bit";
        public static readonly string CrLf = "\r\n";

        private static string HexDecoderEvaluator(Match m)
        {
            var hex = m.Groups[2].Value;
            var iHex = Convert.ToInt32(hex, 16);
            var c = (char)iHex;
            return c.ToString();
        }

        private static string HexDecoder(string line)
        {
            if (line == null)
                throw new ArgumentNullException();

            //parse looking for =XX where XX is hexadecimal
            var re = new Regex("(\\=([0-9A-F][0-9A-F]))",RegexOptions.IgnoreCase);
            return re.Replace(line, new MatchEvaluator(HexDecoderEvaluator));
        }

        public static string QuotedPrintableDecode(string encoded)
        {
            if (encoded == null)
                throw new ArgumentNullException();

            string line;
            var sw = new StringWriter();
            var sr = new StringReader(encoded);
            try
            {
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.EndsWith("="))
                        sw.Write(HexDecoder(line.Substring(0, line.Length - 1)));
                    else
                        sw.WriteLine(HexDecoder(line));

                    sw.Flush();
                }
                return sw.ToString();
            }
            finally
            {
                sw.Close();
                sr.Close();
            }
        }

        public static string DecodeWord(string val)
        {
            var result = val;
            var re = new Regex(@"(=\?)(iso-[0-9]+-[A-Za-z0-9]+\?[A-Za-z]\?[^?]+)(\?=)?", RegexOptions.IgnoreCase);

            var mc = re.Matches(result);
            if (mc.Count > 0)
            {
                for (var i = 0; i < mc.Count; i++)
                {
                    var enc_val = mc[i].ToString();
                    var dec_val = enc_val;
                    var parts = enc_val.Split(new char[] { '?' });

                    if (parts.Length != 5) continue;
                    
                    var encoding = parts[1];
                    var type = parts[2];
                    var datax = parts[3];

                    var enc = GetEncoding(encoding);
                    if (type.ToUpper() == "Q")
                    {
                        dec_val = enc.GetString(enc.GetBytes(QuotedPrintableDecode(datax)));
                    }

                    if (type.ToUpper() == "B")
                    {
                        dec_val = enc.GetString(Convert.FromBase64String(datax));
                    }

                    result = result.Replace(enc_val, dec_val);
                }
            }
            else
            {
                var enc = GetEncoding();

                try
                {
                    result = enc.GetString(enc.GetBytes(result));
                }
                catch
                {
                    // 変な文字列の場合は例外発生なんで空文字列にする
                    result = "";
                }
            }

            return (result);
        }

        public static byte[] ParseData(string encoding, string charset, string mimeDataEntry)
        {
            switch (encoding.ToLower())
            {
                case "quoted-printable":
                    var bp = QuotedPrintableDecode(mimeDataEntry);
                    return GetEncoding(charset).GetBytes(bp);

                case "7bit":
                    return Encoding.ASCII.GetBytes(mimeDataEntry);

                case "8bit":
                    return GetEncoding(charset).GetBytes(mimeDataEntry);

                case "base64":
                    return Convert.FromBase64String(mimeDataEntry);

                default:
                    // default 7bit
                    return Encoding.ASCII.GetBytes(mimeDataEntry);
            }
        }

        public static string DecodeString(string encoding, string charset, string mimeDataEntry)
        {
            var bytes = ParseData(encoding, charset, mimeDataEntry);
            var body = GetEncoding(charset).GetString(bytes);

            return body;
        }

        public static byte[] DecodeBase64(string mimeDataEntry)
        {
            return ParseData("base64", "", mimeDataEntry);
        }

        public static Encoding GetEncoding()
        {
            return GetEncoding(DefaultCharset);
        }

        public static Encoding GetEncoding(string charset)
        {
            Encoding encoding;
            try
            {
                encoding = Encoding.GetEncoding(charset);
            }
            catch
            {
                // システムでサポートしてないCharsetならデフォルト値を使用する。
                encoding = Encoding.GetEncoding(DefaultCharset);
            }

            return encoding;
        }

            /// <summary>
    /// 文字コードを判別する
    /// </summary>
    /// <remarks>
    /// Jcode.pmのgetcodeメソッドを移植したものです。
    /// Jcode.pm(http://openlab.ring.gr.jp/Jcode/index-j.html)
    /// </remarks>
    /// <param name="byts">文字コードを調べるデータ</param>
    /// <returns>適当と思われるEncodingオブジェクト。
    /// 判断できなかった時はnull。</returns>
    public static System.Text.Encoding GetBytesEncoding(byte[] byts)
    {
      const byte bESC = 0x1B;
      const byte bAT = 0x40;
      const byte bDollar = 0x24;
      const byte bAnd = 0x26;
      const byte bOP = 0x28;    //(
      const byte bB = 0x42;
      const byte bD = 0x44;
      const byte bJ = 0x4A;
      const byte bI = 0x49;

      int len = byts.Length;
      int binary = 0;
      int ucs2 = 0;
      int sjis = 0;
      int euc = 0;
      int utf8 = 0;
      byte b1, b2;

      for (int i = 0; i < len; i++)
      {
        if (byts[i] <= 0x06 || byts[i] == 0x7F || byts[i] == 0xFF)
        {
          //'binary'
          binary++;
          if (len - 1 > i && byts[i] == 0x00
            && i > 0 && byts[i - 1] <= 0x7F)
          {
            //smells like raw unicode
            ucs2++;
          }
        }
      }

      if (binary > 0)
      {
        if (ucs2 > 0)
          //JIS
          //ucs2(Unicode)
          return System.Text.Encoding.Unicode;
        else
          //binary
          return null;
      }

      for (int i = 0; i < len - 1; i++)
      {
        b1 = byts[i];
        b2 = byts[i + 1];

        if (b1 == bESC)
        {
          if (b2 >= 0x80)
            //not Japanese
            //ASCII
            return System.Text.Encoding.ASCII;
          else if (len - 2 > i &&
            b2 == bDollar && byts[i + 2] == bAT)
            //JIS_0208 1978
            //JIS
            return System.Text.Encoding.GetEncoding(50220);
          else if (len - 2 > i &&
            b2 == bDollar && byts[i + 2] == bB)
            //JIS_0208 1983
            //JIS
            return System.Text.Encoding.GetEncoding(50220);
          else if (len - 5 > i &&
            b2 == bAnd && byts[i + 2] == bAT && byts[i + 3] == bESC &&
            byts[i + 4] == bDollar && byts[i + 5] == bB)
            //JIS_0208 1990
            //JIS
            return System.Text.Encoding.GetEncoding(50220);
          else if (len - 3 > i &&
            b2 == bDollar && byts[i + 2] == bOP && byts[i + 3] == bD)
            //JIS_0212
            //JIS
            return System.Text.Encoding.GetEncoding(50220);
          else if (len - 2 > i &&
            b2 == bOP && (byts[i + 2] == bB || byts[i + 2] == bJ))
            //JIS_ASC
            //JIS
            return System.Text.Encoding.GetEncoding(50220);
          else if (len - 2 > i &&
            b2 == bOP && byts[i + 2] == bI)
            //JIS_KANA
            //JIS
            return System.Text.Encoding.GetEncoding(50220);
        }
      }

      for (int i = 0; i < len - 1; i++)
      {
        b1 = byts[i];
        b2 = byts[i + 1];
        if (((b1 >= 0x81 && b1 <= 0x9F) || (b1 >= 0xE0 && b1 <= 0xFC)) &&
          ((b2 >= 0x40 && b2 <= 0x7E) || (b2 >= 0x80 && b2 <= 0xFC)))
        {
          sjis += 2;
          i++;
        }
      }
      for (int i = 0; i < len - 1; i++)
      {
        b1 = byts[i];
        b2 = byts[i + 1];
        if (((b1 >= 0xA1 && b1 <= 0xFE) && (b2 >= 0xA1 && b2 <= 0xFE)) ||
          (b1 == 0x8E && (b2 >= 0xA1 && b2 <= 0xDF)))
        {
          euc += 2;
          i++;
        }
        else if (len - 2 > i &&
          b1 == 0x8E && (b2 >= 0xA1 && b2 <= 0xFE) &&
          (byts[i + 2] >= 0xA1 && byts[i + 2] <= 0xFE))
        {
          euc += 3;
          i += 2;
        }
      }
      for (int i = 0; i < len - 1; i++)
      {
        b1 = byts[i];
        b2 = byts[i + 1];
        if ((b1 >= 0xC0 && b1 <= 0xDF) && (b2 >= 0x80 && b2 <= 0xBF))
        {
          utf8 += 2;
          i++;
        }
        else if (len - 2 > i &&
          (b1 >= 0xE0 && b1 <= 0xEF) && (b2 >= 0x80 && b2 <= 0xBF) &&
          (byts[i + 2] >= 0x80 && byts[i + 2] <= 0xBF))
        {
          utf8 += 3;
          i += 2;
        }
      }

      if (euc > sjis && euc > utf8)
        //EUC
        return System.Text.Encoding.GetEncoding(51932);
      else if (sjis > euc && sjis > utf8)
        //SJIS
        return System.Text.Encoding.GetEncoding(932);
      else if (utf8 > euc && utf8 > sjis)
        //UTF8
        return System.Text.Encoding.UTF8;

      return null;
    }

    }
}
