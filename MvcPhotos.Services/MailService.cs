using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MvcPhotos.Mail;
using MvcPhotos.Models;

namespace MvcPhotos.Services
{
    public class MailService : IDisposable
    {
        private readonly IPhotosRepository _repository;

        private readonly bool _secure;
        private readonly string _server;
        private readonly int _port;
        private readonly string _userName;
        private readonly string _password;

        public MailService() : this(ObjectResolver.Resolve<IPhotosRepository>())
        {
        }

        public MailService(IPhotosRepository repository)
        {
            _secure = bool.Parse(ConfigurationManager.AppSettings["Pop3.Secure"] ?? "false");
            _server = ConfigurationManager.AppSettings["Pop3.Server"];
            _port = int.Parse(ConfigurationManager.AppSettings["Pop3.Port"] ?? "110");
            _userName = ConfigurationManager.AppSettings["Pop3.UserName"];
            _password = ConfigurationManager.AppSettings["Pop3.Password"];

            _repository = repository;
        }

        public static string GetMailAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            //RFC2822じゃないよ！
            // ケータイの自由奔放さをなめるなよ！
            var match = Regex.Match(value, @"[a-zA-Z0-9!$&*.=^`|~#%'+\/?_{}-]+@([a-zA-Z0-9_-]+\.)+[a-zA-Z]{2,4}");
            if (!match.Success)
                return null;

            return match.Value;
        }

        private static Photo InternalSaveMultipart(string uploadingPath, Multipart multipart, string name)
        {
            var photo = new Photo
                            {
                                FileName = PhotosService.InitializeSavePath(uploadingPath, name)
                            };

            var path = Path.Combine(uploadingPath, photo.FileName);

            multipart.SaveToFile(path);
            
            photo.TakenAt = ImageResizer.GetExifTakenAt(path);
            photo.EntryAt = DateTime.Now;
            photo.ContentType = multipart.Body.ContentType;
            photo.Length = multipart.Length;

            return photo;
        }

        private void InternalReceive(string uploadingPath, Pop3 pop3, MessageItem item)
        {
            Trace.WriteLine(string.Format(" retr mail {0}  : {1}", item.No, DateTime.Now));
            var message = pop3.CommandRetr(item.No);
            var delete = true;
            if (message.Successed)
            {
                var subject = "メール投稿 " + message.Message.Header("Subject");
                var sender = message.Message.Header("From");
                var senderHash = PhotosService.GetStringHash(sender);
                var messageId = message.Message.Header("Message-ID");

                var textPart = message.Message.FirstTextPlainMultipart();
                var text = textPart != null 
                    ? Mime.DecodeString(textPart.Encoding, textPart.Charset, textPart.BodySource).Trim()
                    : "";
                var description = text;

                if (_repository.GetPhoto(messageId) == null)
                {
                    var images = message.Message
                        .Multiparts
                        .Where(m => m.Body.ContentType.StartsWith("image/"));

                    Trace.WriteLine(string.Format(" save photos {0}({1})  : {2}", item.No, images.Count(), DateTime.Now));
                    foreach (var image in images)
                    {
                        var ext = ImageResizer.GetMimeToExtension(image.Body.ContentType);
                        var photo = InternalSaveMultipart(uploadingPath, image, string.Format("original.{0}", ext));
                        photo.ImportTags(Tag.ToTags(subject.Split(' ')));
                        photo.MessageId = messageId;
                        photo.Description = description;

                        // メールアドレスを削除コードにする。
                        photo.DeleteCode = senderHash;
                        photo.Sender = senderHash;

                        _repository.Append(photo);
                        _repository.SaveChanges();
                    }
                }
                else
                {
                    // 他のホストで取り出し中のはず
                    delete = false;
                }
            }
            
            if (delete)
                pop3.CommandDele(item.No);

            Trace.WriteLine(string.Format(" dele mail {0}  : {1}", item.No, DateTime.Now));
        }

        public void Receive(string uploadingPath)
        {
            if(string.IsNullOrEmpty(uploadingPath))
                throw new ArgumentNullException("uploadingPath");
            if(!Directory.Exists(uploadingPath))
                throw new ArgumentException("一時保存用のフォルダがありません", "uploadingPath");

            // 受信してファイルに保存後、DBにメタ情報を登録
            using(var pop3 = new Pop3())
            {
                var connected = pop3.Connect(_server, _port, _secure);
                if(!connected.Successed)
                    return;

                var loggedIn = pop3.Login(_userName, _password);
                if(!loggedIn.Successed)
                    return;

                var received = pop3.GetMessageItems();
                if(!received.Successed)
                    return;

                foreach (var item in received.Items)
                {
                    InternalReceive(uploadingPath, pop3, item);
                }

                // 削除の確定
                pop3.CommandQuit();
            }
        }

        public void Dispose()
        {
            var repository = _repository as IDisposable;

            if(repository!=null)
                repository.Dispose();
        }
    }
}
