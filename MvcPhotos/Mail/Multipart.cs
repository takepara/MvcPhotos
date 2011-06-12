using System.IO;
using System.Text.RegularExpressions;

namespace MvcPhotos.Mail
{
    public class Multipart
    {
        private readonly int _sourcePosition;
        private readonly int _length;
        private readonly Message _sourceMessage;

        public Multipart(Message message, int spos, int len)
        {
            // 
            // TODO: コンストラクタ ロジックをここに追加してください。
            //
            _sourceMessage = message;
            _sourcePosition = spos;
            _length = len;
        }

        public Message Body
        {
            get
            {
                Message result = null;

                if (Length != -1)
                {
                    result = new Message
                                 {
                                     Source = _sourceMessage.Source.Substring(StartPosition, Length)
                                 };
                }

                return (result);
            }
        }

        public int StartPosition
        {
            get
            {
                return (_sourcePosition);
            }
        }

        public int Length
        {
            get
            {
                return (_length);
            }
        }

        public bool IsTextPlain
        {
            get
            {
                var hpos = HeaderLength();
                var hasHeader = false;
                // ヘッダ部あり
                if (hpos > 0)
                {
                    var re = new Regex("content-type:( |\t|)text/plain;", RegexOptions.IgnoreCase);
                    var rem = re.Match(_sourceMessage.Source, _sourcePosition, hpos);
                    hasHeader = rem.Success;
                }
                
                // ヘッダ部なし
                // 改行と空白しかない場合は空っぽパート扱いにするっす。
                var body = _sourceMessage.Source.Substring(_sourcePosition, _length);

                return hasHeader && !string.IsNullOrWhiteSpace(body);
            }
        }

        public bool IsAttachment
        {
            get
            {
                int hpos = HeaderLength();
                // ヘッダ部あり
                if (hpos > 0)
                {
                    var result = false;

                    // content-dispositionがあれば添付とする
                    var re = new Regex("content-disposition:( |\t|)attachment;", RegexOptions.IgnoreCase);
                    var rem = re.Match(_sourceMessage.Source, _sourcePosition, hpos);
                    if (rem.Success)
                    {
                        result = true;
                    }

                    // なくてもcontent-typeにname値があれば添付としようかな
                    if (result == false)
                    {
                        if (Body.HeaderSplitValues("content-type").ContainsKey("name"))
                        {
                            result = true;
                        }
                    }

                    return result;
                }
                // ヘッダ部なしなら添付じゃない
                return false;
            }
        }

        public byte[] GetBytes()
        {
            return (Mime.DecodeBase64(Body.BodySource));
        }

        public bool SaveToFile(string filename)
        {
            var binary = GetBytes();

            using (var fs = new FileStream(filename, FileMode.Create))
                fs.Write(binary, 0, binary.Length);

            return (true);
        }

        private int HeaderLength()
        {
            var hpos = _sourceMessage.Source.IndexOf(Mime.CrLf + Mime.CrLf, _sourcePosition);
            if (hpos != -1)
            {
                hpos -= _sourcePosition;
            }

            return (hpos);
        }
    }
}
