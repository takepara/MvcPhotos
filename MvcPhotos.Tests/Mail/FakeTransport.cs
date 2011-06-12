using System.Collections.Generic;
using System.IO;
using MvcPhotos.Mail;

namespace MvcPhotos.Tests.Mail
{
    public class FakeTransport : ITransport
    {
        // http://seo.atompro.net/webtoolfree_strcnva_.html
        private Dictionary<string, string> _responseText = new Dictionary<string, string>
        {
            {"TextOnly",
@"+OK
Return-Path: takepara@gmail.com
Received: from CW-PC-087 ([220.98.6.245])
	by dnmail01.datajapan.ne.jp
	; Wed, 27 Apr 2011 17:49:48 +0900
Message-ID: <7DAB2F38-9BC0-4EAD-9503-830EE37004C6@dnmail01.datajapan.ne.jp>
MIME-Version: 1.0
From: takepara@gmail.com
To: pop3test@takepara.com
Date: 27 Apr 2011 17:49:45 +0900
Subject: =?utf-8?B?44OG44K544OI44Oh44O844Or5Lu25ZCN?=
Content-Type: text/plain; charset=utf-8
Content-Transfer-Encoding: base64

44OG44K544OI44Oh44O844Or44Gu5pys5paHDQoyMDExLzA0LzI3IDE3OjQ4OjQw

.
"},{"TextOnlyAttatch",
@"+OK 1904 octets
Return-Path: takepara@gmail.com
Received: from mail-qw0-f54.google.com ([209.85.216.54])
        by dnmail01.datajapan.ne.jp
        ; Thu, 28 Apr 2011 00:13:17 +0900
Received: by qwc9 with SMTP id 9so997021qwc.41
        for <pop3test@takepara.com>; Wed, 27 Apr 2011 08:13:15 -0700 (PDT)
DKIM-Signature: v=1; a=rsa-sha256; c=relaxed/relaxed;
        d=gmail.com; s=gamma;
        h=domainkey-signature:mime-version:date:message-id:subject:from:to
         :content-type;
        bh=f4fa6+wol6ctLViDNDg5CfEew0yTCeAzAyNpBQycmAk=;
        b=o7UIhQQCG9nzDZ+S9WaS8Ph4Pdq14ZGoAnl0mPLx7g8Vtft5His89RXylOy7I3PQ8f
         gMVgXI8+e+Fa8z4R4QuzNAfXkfW2O+iV+Yg5Trl5O1phZzPAoh/dnTFWdJPYrXYnqw1z
         f1ECZRTulTjZFm0hnkuP10iD2wiaZYQ/6vgUk=
DomainKey-Signature: a=rsa-sha1; c=nofws;
        d=gmail.com; s=gamma;
        h=mime-version:date:message-id:subject:from:to:content-type;
        b=pLjftKADzoUS70Sb+ekw9gwvadwQ3U4qbXMGbq36lQ9gVPqCph8mw/0IjrzNt6B7pp
         s0LCliI0I4avdTINo9jehkVnhuKZw8MhtQ1yBmSn1FP6t60yWbZaFTmzm8irV6LdOdkN
         zvR+ePl3JpvRW6SFeaMxCQVB5M6ADBp+P/eGQ=
MIME-Version: 1.0
Received: by 10.229.106.32 with SMTP id v32mr1826983qco.77.1303917195508; Wed,
 27 Apr 2011 08:13:15 -0700 (PDT)
Received: by 10.229.235.9 with HTTP; Wed, 27 Apr 2011 08:13:15 -0700 (PDT)
Date: Thu, 28 Apr 2011 00:13:15 +0900
Message-ID: <BANLkTi=tcd=guKR4zBWqBWRj21NUTNupsg@mail.gmail.com>
Subject:
From: =?ISO-2022-JP?B?GyRCJD8kMSRPJGkbKEI=?= <takepara@gmail.com>
To: pop3test@takepara.com
Content-Type: multipart/mixed; boundary=00235429dbc4413d8104a1e7e14b

--00235429dbc4413d8104a1e7e14b
Content-Type: text/plain; charset=Shift_JIS; name=""test.txt""
Content-Disposition: attachment; filename=""test.txt""
Content-Transfer-Encoding: base64
X-Attachment-Id: f_gn0emn0p0

k1mVdINlg0yDWINngsyDZYNYg2c=
--00235429dbc4413d8104a1e7e14b--
."},{"TextAndTextAttatch",
@"+OK 2020 octets
Return-Path: takepara@gmail.com
Received: from mail-qy0-f182.google.com ([209.85.216.182])
        by dnmail01.datajapan.ne.jp
        ; Thu, 28 Apr 2011 00:21:56 +0900
Received: by qyk27 with SMTP id 27so967772qyk.20
        for <pop3test@takepara.com>; Wed, 27 Apr 2011 08:21:55 -0700 (PDT)
DKIM-Signature: v=1; a=rsa-sha256; c=relaxed/relaxed;
        d=gmail.com; s=gamma;
        h=domainkey-signature:mime-version:date:message-id:subject:from:to
         :content-type;
        bh=L1VWBTkAWLumJB5EHIhSui/78Uy4TBCasfLnLr7cD6Y=;
        b=jey2VLZ6GE1NfNCtkHimThBmiG+pa4vF2E/CIcBxdiRTpNNIHLv8cXjpwyCl06eoUg
         r3oDq1/kzbHpVDq+sPdIVfI721NPzinmWc98m4DVneSkLvz1qqqcQZ0rP5RJdfssZ5ZE
         oAoyly44pLkYy5U28ZnCrliGB+vaMK2p1r6os=
DomainKey-Signature: a=rsa-sha1; c=nofws;
        d=gmail.com; s=gamma;
        h=mime-version:date:message-id:subject:from:to:content-type;
        b=xIrQK5dmArH1XM8KJbp1lEJYEF5wz9ld2umi+gpEURU5lWD5/rfgcxUEM/hUQS/zNJ
         KYXQYCHeDU7zRkSuR6LgL/qH71Uy3veV2DEOvFlxfN7gBQ3/KmsGu0UUZsWpXXTmO5cN
         VPbrk9UFBRjIymrOJHB+8R0aG/ymxsPvjN3AY=
MIME-Version: 1.0
Received: by 10.229.86.76 with SMTP id r12mr1828538qcl.115.1303917715259; Wed,
 27 Apr 2011 08:21:55 -0700 (PDT)
Received: by 10.229.235.9 with HTTP; Wed, 27 Apr 2011 08:21:55 -0700 (PDT)
Date: Thu, 28 Apr 2011 00:21:55 +0900
Message-ID: <BANLkTin1qyhw0JU4NxgHnMJXnSF3+TkDOQ@mail.gmail.com>
Subject: =?ISO-2022-JP?B?GyRCJUYlLSU5JUgkSCVGJS0lOSVIRTpJVRsoQg==?=
From: =?ISO-2022-JP?B?GyRCJD8kMSRPJGkbKEI=?= <takepara@gmail.com>
To: pop3test@takepara.com
Content-Type: multipart/mixed; boundary=0016364ee2ec3c01cb04a1e80014

--0016364ee2ec3c01cb04a1e80014
Content-Type: text/plain; charset=ISO-2022-JP
Content-Transfer-Encoding: 7bit

$BK\J8$b$""$k$h!#(B

--0016364ee2ec3c01cb04a1e80014
Content-Type: text/plain; charset=Shift_JIS; name=""test.txt""
Content-Disposition: attachment; filename=""test.txt""
Content-Transfer-Encoding: base64
X-Attachment-Id: f_gn0exjg00

k1mVdINlg0yDWINngsyDZYNYg2c=
--0016364ee2ec3c01cb04a1e80014--
."},{"ImageAttatch",
@"+OK 6260 octets
Return-Path: takepara@gmail.com
Received: from mail-qy0-f175.google.com ([209.85.216.175])
        by dnmail01.datajapan.ne.jp
        ; Thu, 28 Apr 2011 01:42:42 +0900
Received: by qyk35 with SMTP id 35so1934270qyk.20
        for <pop3test@takepara.com>; Wed, 27 Apr 2011 09:42:41 -0700 (PDT)
DKIM-Signature: v=1; a=rsa-sha256; c=relaxed/relaxed;
        d=gmail.com; s=gamma;
        h=domainkey-signature:mime-version:date:message-id:subject:from:to
         :content-type;
        bh=wuVyqtvQoluVr6ire5IZv5unuahjQvsPOkU4dw+ks+s=;
        b=qMqtwsC3PVOxr7VOd45I3+KEQMFz0Zo7d32z3kn1r3YfD0kNZN6y1XPkKL4qGIeEBH
         YNrZoymGe+nIXwi3kYbYcjbSgrX3v1OrkUYSpgkL9BK7KIJth8xTarWRMdFhAyHXe4c8
         2DvKbII06Yq8lgfgykTrOjlpzRbQrUspD63Xo=
DomainKey-Signature: a=rsa-sha1; c=nofws;
        d=gmail.com; s=gamma;
        h=mime-version:date:message-id:subject:from:to:content-type;
        b=aOMwElOZoUp/aUxvfsmi/OwgqyXB5MfCtw+0SsSQpL1MXVOO/PdMl12P2slUTsZ+IC
         5qhcX9Hi7OAMmEA0Jg/9wOKCG0ywzy7mc5wAPA3KopfLa0IpVetLVI5arVJR387PUm9H
         czmf8HU6XueoO4YzdLAWTtwf+NxaVsiItOKg8=
MIME-Version: 1.0
Received: by 10.229.86.76 with SMTP id r12mr1900366qcl.115.1303922560903; Wed,
 27 Apr 2011 09:42:40 -0700 (PDT)
Received: by 10.229.235.9 with HTTP; Wed, 27 Apr 2011 09:42:40 -0700 (PDT)
Date: Thu, 28 Apr 2011 01:42:40 +0900
Message-ID: <BANLkTinA9Z=HVCwnSa8gTs6vo9dwFjrGoA@mail.gmail.com>
Subject:
From: =?ISO-2022-JP?B?GyRCJD8kMSRPJGkbKEI=?= <takepara@gmail.com>
To: pop3test@takepara.com
Content-Type: multipart/mixed; boundary=0016364ee2ec0edc5b04a1e921e4

--0016364ee2ec0edc5b04a1e921e4
Content-Type: text/plain; charset=ISO-8859-1



--0016364ee2ec0edc5b04a1e921e4
Content-Type: image/jpeg; name=""metalking_s.jpg""
Content-Disposition: attachment; filename=""metalking_s.jpg""
Content-Transfer-Encoding: base64
X-Attachment-Id: f_gn0htlyz0

/9j/4AAQSkZJRgABAQEAYABgAAD/4QCyRXhpZgAASUkqAAgAAAAFABoBBQABAAAASgAAABsBBQAB
AAAAUgAAACgBAwABAAAAAgAAADEBAgAQAAAAWgAAAGmHBAABAAAAagAAAAAAAABgAAAAAQAAAGAA
AAABAAAAUGFpbnQuTkVUIFYzLjM2AAEAhpICAC4AAAB8AAAAAAAAAE9QVFBpWCB3ZWJEZXNpZ25l
ciAgaHR0cDovL3d3dy53ZWJ0ZWNoLmNvLmpwLwD/2wBDAAMCAgMCAgMDAwMEAwMEBQgFBQQEBQoH
BwYIDAoMDAsKCwsNDhIQDQ4RDgsLEBYQERMUFRUVDA8XGBYUGBIUFRT/2wBDAQMEBAUEBQkFBQkU
DQsNFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBT/wAAR
CABgAGADASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgED
AwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRol
JicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWW
l5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3
+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3
AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5
OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaan
qKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIR
AxEAPwD3YIPqakEeR6CnJGBjJp7DHQ8V4JzkRhHemPCD1qXdxRkGmMg+xqab9lUVYyBz1rhL34wa
LY+JNR0mUOf7P/4+p15EK7N5YjH3RyCRyNp49Zc1FXkFmztRbgVKkY29KZBcQ3CBopUlX1RgRUB1
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
APSmGQD2piP/2Q==
--0016364ee2ec0edc5b04a1e921e4--
.
"}
        };

        private bool _connected;
        private string _responseCode;

        public FakeTransport(string responseCode)
        {
            _responseCode = responseCode;
        }

        public bool Connect(string server, int port)
        {
            return Connect(server, port, false);
        }

        public bool Connect(string server, int port, bool isSecure)
        {
            return _connected = true;
        }

        public bool Connected
        {
            get { return _connected; }
        }

        public bool IsValidResponse(string responseText)
        {
            return responseText.StartsWith("+");
        }

        public void SendCommand(string command)
        {
        }

        public IEnumerable<string> ReadLine()
        {
            using (var reader = new StringReader(_responseText[_responseCode]))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
