using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MvcPhotos.Mail
{
    public class Pop3 : IDisposable
    {
        // ちゃんとServiceLocatorとしてObjectResolverクラス用意したからそれ使うのが
        // いいんだな～。でもサンプルだから違うパターンも作っておくよ。
        // 静的フィールドにFunc<T>を保持する形でのDI実装。
        public static Func<ITransport> TransportFactory = () => new Transport();
        
        private readonly ITransport _transport;
        private string _apopCharenge = "";

        public Pop3()
        {
            _transport = TransportFactory();
        }

        public Response Connect(string server, int port, bool isSecure)
        {
            var response = new Response();

            try
            {
                // initialization
                _transport.Connect(server, port, isSecure);

                response.Successed = true;
                response.Trace = _transport.ReadLine().FirstOrDefault();

                // Apop?
                var match = Regex.Match(response.Trace, "<.+>");
                if (match.Success)
                    _apopCharenge = match.Value;
            }
            catch (Exception err)
            {
                response.Trace = "Error: " + err;
            }
            return response;
        }

        public Response Login(string userid, string password)
        {
            return Login(userid, password, false);
        }

        public string ComputeApopHash(string charenge, string password)
        {
            var target = charenge + password;

            MD5 md5 = new MD5CryptoServiceProvider();
            var bytes = md5.ComputeHash(Encoding.ASCII.GetBytes(target));

            return string.Join("", bytes.Select(b => b.ToString("x2")));
        }

        public Response Login(string userid, string password, bool apop)
        {
            Response response;

            if (apop)
            {
                var digest = ComputeApopHash(_apopCharenge, password);
                response = GetResponse("APOP " + userid + " " + digest);
            }
            else
            {
                response = GetResponse("USER " + userid);
                if (!response.Successed)
                    return response;

                var message = response.Trace;
                response = GetResponse("PASS " + password);
                response.Trace = message + "\r\n" + response.Trace;
            }

            return (response);
        }

        public Response GetMessageItems()
        {
            var response = new Response();
            var uidl = CommandUidl();
            var list = CommandList();
            if (!uidl.Successed || !list.Successed)
                return response;

            if (uidl.Items.Count != list.Items.Count)
                return response;

            for (var i = 0; i < uidl.Items.Count; i++)
            {
                var item = new MessageItem
                                {
                                    No = uidl.Items[i].No,
                                    Text = uidl.Items[i].Text,
                                    Value = int.Parse(list.Items[i].Text)
                                };

                response.Items.Add(item);
            }
            response.Trace = "+OK";
            response.Successed = true;

            return response;
        }

        private Response GetResponse(string cmd)
        {
            var result = new Response();

            try
            {
                _transport.SendCommand(cmd);
                result.Trace = _transport.ReadLine().FirstOrDefault();
                result.Successed = _transport.IsValidResponse(result.Trace);
            }
            catch (Exception err)
            {
                result.Successed = false;
                result.Trace = "Error in GetResponse: " + err;
            }

            return (result);
        }

        private Response GetResponses(string cmd)
        {
            var result = new Response();
            string readLine;

            try
            {
                var sb = new StringBuilder();
                _transport.SendCommand(cmd);

                readLine = _transport.ReadLine().FirstOrDefault();
                result.Trace = readLine;
                result.Successed = _transport.IsValidResponse(result.Trace);

                if (result.Successed)
                {
                    sb.Length = 0;
                    // サーバーからのデータ本体を取得（ドットだけの行がくるまでね）
                    foreach (var line in _transport.ReadLine())
                    {
                        if (line == ".")
                            break;

                        if (sb.Length > 0)
                            sb.Append("\r\n");

                        // 行の先頭がピリオド２文字ならひとつ消す。
                        sb.Append(line.StartsWith("..") ? line.Substring(1) : line);
                    }
                }
                result.Body = sb.ToString();
            }
            catch (Exception err)
            {
                result.Successed = false;
                result.Trace = "Error in GetResponses(): " + err;
            }

            return result;
        }

        private Response CommandInternal(string command, int? no)
        {
            var index = no.HasValue ? 0 : 1;
            var result = !no.HasValue
                             ? GetResponses(command)
                             : GetResponse(command + (no.HasValue ? " " + no : ""));
            if (result.Successed)
            {
                var reader = new StringReader(result.Body);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var cols = line.Split(' ');
                    if (cols.Length < (3 - index)) continue;

                    var uid = new MessageItem { No = Convert.ToInt32(cols[1 - index]), Text = cols[2 - index] };
                    result.Items.Add(uid);
                }
            }

            return result;
        }

        public Response CommandUidl()
        {
            return CommandInternal("UIDL", null);
        }

        public Response CommandUidl(int no)
        {
            return CommandInternal("UIDL", no);
        }

        public Response CommandList()
        {
            return CommandInternal("LIST", null);
        }

        public Response CommandList(int no)
        {
            return CommandInternal("LIST", no);
        }

        public Response CommandRetr(int no)
        {
            var result = GetResponses("RETR " + no);
            if (result.Successed)
            {
                result.Message.Source = result.Body;
            }

            return result;
        }

        public Response CommandTop(int no)
        {
            var result = GetResponses("TOP " + no);
            if (result.Successed)
            {
                result.Message.Source = result.Body;
            }

            return result;
        }

        public Response CommandTop(int no, int top)
        {
            var result = GetResponses("TOP " + no + " " + top);
            if (result.Successed)
            {
                result.Message.Source = result.Body;
            }

            return result;
        }

        public Response CommandDele(int no)
        {
            return GetResponse("DELE " + no);
        }

        public Response CommandQuit()
        {
            return GetResponse("QUIT");
        }

        public void Dispose()
        {
            if (_transport is IDisposable)
                ((IDisposable)_transport).Dispose();
        }
    }
}
