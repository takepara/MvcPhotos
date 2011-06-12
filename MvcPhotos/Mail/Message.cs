using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MvcPhotos.Mail
{
    public class Message
    {
        private string _source = "";
        private List<Multipart> _multiparts;
        private bool _isMultipart;
        private string _boundary = "";
        private int _bodyPosition;


        public string Source
        {
            get
            {
                return _source;
            }
            set
            {
                // メッセージソース
                _source = value;

                // マルチパートデータ
                _multiparts = new List<Multipart>();

                // マルチパート？
                _isMultipart = HasMultipartHeader;

                // 本文開始位置
                _bodyPosition = _source.IndexOf(Mime.CrLf + Mime.CrLf);
                if (_bodyPosition != -1)
                {
                    // 改行２個分をシーク
                    _bodyPosition += 4;
                }

                // バウンダリ文字列
                _boundary = BoundaryHeader;

                // マルチパートメールならマルチパート部の情報を解析する
                if (_isMultipart)
                {
                    string boundary = "--" + _boundary;
                    if (_bodyPosition != -1 && boundary != "")
                    {
                        int spos = _bodyPosition;

                        // 本文中に存在するboundary文字列の位置と数を数える
                        while (true)
                        {
                            spos = _source.IndexOf(boundary, spos);
                            int epos;
                            if (spos != -1)
                            {
                                // Multipart前に本文が存在する場合はここで取得
                                int mpLen;
                                int mpSpos;
                                if (_multiparts.Count == 0 && spos > _bodyPosition)
                                {
                                    mpSpos = _bodyPosition;
                                    mpLen = spos - _bodyPosition;

                                    _multiparts.Add(new Multipart(this, mpSpos, mpLen));
                                }

                                // Multipart本体の検索
                                epos = _source.IndexOf(boundary, spos + boundary.Length + Mime.CrLf.Length);
                                if (epos != -1)
                                {
                                    mpSpos = spos + boundary.Length + Mime.CrLf.Length;
                                    mpLen = epos - 2 - mpSpos;
                                }
                                else
                                {
                                    break;
                                }

                                // マルチパート部の確保
                                _multiparts.Add(new Multipart(this, mpSpos, mpLen));
                            }
                            else
                            {
                                break;
                            }
                            spos = epos - 2;
                        }
                    }
                }
            }
        }

        public string BodySource
        {
            get
            {
                if (_bodyPosition != -1)
                {
                    return _source.Substring(_bodyPosition);
                }

                return ("");
            }
        }

        public string Header(string headerName)
        {
            var sr = new StringReader(_source);
            string readLine;
            string targetHeader = "";
            string targetResult = "";

            while (true)
            {
                readLine = sr.ReadLine();
                // HEADER部の終了かメッセージの終了なら処理中断
                if (string.IsNullOrEmpty(readLine))
                {
                    break;
                }

                // 行頭がスペースかタブなら前のヘッダ行の続き
                if (readLine.StartsWith(" ") || readLine.StartsWith("\t"))
                {
                    if (targetHeader != "")
                    {
                        targetResult += readLine.Trim();
                    }
                }
                else
                {
                    // 発見後はそこで中断
                    if (targetHeader != "")
                    {
                        break;
                    }

                    // ヘッダ名を取得
                    int spos = readLine.IndexOf(":", 0);
                    if (spos != -1)
                    {
                        targetHeader = readLine.Substring(0, spos).ToLower();
                        // ヘッダー発見
                        if (targetHeader == headerName.ToLower())
                        {
                            // ヘッダの値を取得
                            targetResult = readLine.Substring(spos + 1).Trim();
                        }
                        // 違うヘッダー
                        else
                        {
                            targetHeader = "";
                        }
                    }
                }
            }
            targetResult = Mime.DecodeWord(targetResult);

            return (targetResult);
        }

        public Dictionary<string, string> HeaderSplitValues(string headerName)
        {
            var ht = new Dictionary<string, string>();

            string pHeader = Header(headerName);
            string[] pVals = pHeader.Split(';');
            if (pVals.Length > 0)
            {
                foreach (string t in pVals)
                {
                    string[] pVal = t.Split(new[] { '=' }, 2);
                    if (pVal.Length == 2)
                    {
                        ht[pVal[0].Trim()] = pVal[1].Replace("\"", "").Trim();
                    }
                    else
                    {
                        ht[t.Trim()] = t.Trim();
                    }
                }
            }

            return (ht);
        }

        public string Charset
        {
            get
            {
                var hv = HeaderSplitValues("content-type");
                var result = hv["charset"];

                if (string.IsNullOrEmpty(result))
                    result = Mime.DefaultCharset;

                return result;
            }
        }

        public string Encoding
        {
            get
            {
                var encoding = Header("content-transfer-encoding");
                // Content-Transfer-Encodingが無い場合は、7bitとする（いいのかな．．．）
                if (string.IsNullOrEmpty(encoding))
                    encoding = Mime.DefaultEncoding;

                return encoding;
            }
        }

        public string FileName
        {
            get
            {
                // content-disposition を優先するよ～
                var fileName = HeaderSplitValues("content-type").ContainsKey("name")
                    ? HeaderSplitValues("content-type")["name"] : "";
                fileName = HeaderSplitValues("content-disposition").ContainsKey("filename")
                    ? HeaderSplitValues("content-disposition")["filename"] : fileName;
                var rfc2231 = HeaderSplitValues("content-disposition").ContainsKey("filename*")
                    ? HeaderSplitValues("content-disposition")["filename*"] : "";

                if (!string.IsNullOrEmpty(rfc2231) && rfc2231.StartsWith("iso-2022-jp''"))
                {
                    // rf2231デコードしないとね。
                    // 面倒だから端折る。
                }
                return fileName;
            }
        }

        public DateTime Date
        {
            get
            {
                // Date Headerからメール日時を取得
                DateTime date;
                var dateHeader = Header("date");
                int pos = dateHeader.LastIndexOf(" +");
                if (pos == -1)
                {
                    pos = dateHeader.LastIndexOf(" -");
                }

                if (pos != -1)
                {
                    dateHeader = dateHeader.Substring(0, pos);
                }

                if (DateTime.TryParse(dateHeader, out date))
                    return date;

                return DateTime.Now;
            }
        }

        public string ContentType
        {
            get
            {
                var result = Header("content-type");

                if (result.IndexOf(';') > 0)
                    result = result.Split(';')[0];

                return result;
            }
        }

        public string ContentTransferEncoding
        {
            get
            {
                string result = Header("content-transfer-encoding");

                if (result.IndexOf(';') > 0)
                    result = result.Split(';')[0];

                return result;
            }
        }

        public bool IsMultipart
        {
            get
            {
                return _isMultipart;
            }
        }

        public string Boundary
        {
            get
            {
                return _boundary;
            }
        }

        public IEnumerable<Multipart> Multiparts
        {
            get
            {
                return _multiparts;
            }
        }

        public Message FirstTextPlainMultipart()
        {
            var result = (from t in _multiparts where t.IsTextPlain select t.Body).FirstOrDefault();

            // 現在の階層のメッセージからテキスト部検索

            // 見つからなかったら下部がマルチパートかチェックして再検索
            if (result == null)
            {
                foreach (var t in _multiparts.Where(t => t.Body.IsMultipart))
                {
                    result = t.Body.FirstTextPlainMultipart();
                    if (result != null)
                        break;
                }
            }

            return (result);
        }

        private bool HasMultipartHeader
        {
            get
            {
                var header = Header("content-type");
                int spos = header.ToLower().IndexOf("multipart/");
                return spos != -1;
            }
        }

        private string BoundaryHeader
        {
            get
            {
                var hv = HeaderSplitValues("content-type");

                return hv.ContainsKey("boundary") ? hv["boundary"] : "";
            }
        }
    }
}
