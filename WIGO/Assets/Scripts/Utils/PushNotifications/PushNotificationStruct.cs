using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace WIGO.Utility
{
    public readonly struct PushNotificationStruct
    {
        public readonly string title;
        public readonly string body;
        public readonly string from;
        public readonly string link;
        public readonly bool opened_from_push;
        public readonly IDictionary<string, string> data;

        public PushNotificationStruct(string title, string body, string from, string link, bool opened_from_push, IDictionary<string, string> data)
        {
            this.title = title;
            this.body = body;
            this.from = from;
            this.link = link;
            this.opened_from_push = opened_from_push;
            this.data = data;
        }

        public override readonly string ToString()
        {
            StringBuilder output = new();
            output.AppendLine($"Title: {title}");
            output.AppendLine($"Body: {body}");
            output.AppendLine($"From: {from}");
            output.AppendLine($"Link: {link}");
            output.AppendLine($"Opened From Push: {opened_from_push}");
            output.AppendLine("Data:");
            var d = data;
            data?.Keys.ToList().ForEach((key) => output.AppendLine($"  Key - {key} : Value - {d[key]}"));
            output.Append("End Of Struct");
            return output.ToString();
        }
    }
}