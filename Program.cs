using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;

class Program {

    static string _URL = "https://localhost:7180/api/";

    static HttpClient _CLIENT = new HttpClient();

    static Dictionary<string, Func<string[], Task<string>>> _COMMAND_DICT = new () {
        
        {"all", async (args)=>{
            var mismatch = argsCountError("all", 0, args.Length);
            if (mismatch != null) {
                return mismatch;
            }

            return await _CLIENT.GetStringAsync(_URL + "messages");
        }},

        {"get", async (args)=>{
            var mismatch = argsCountError("get", 1, args.Length);
            if (mismatch != null) {
                return mismatch;
            }

            return await _CLIENT.GetStringAsync(_URL + $"messages/{args[0]}");
        }},

        {"add", async (args)=>{
            var mismatch = argsCountError("add", 1, args.Length);
            if (mismatch != null) {
                return mismatch;
            }

            var req = new HttpRequestMessage{
                Content = JsonContent.Create(new {Text = args[0]}),
                Method = HttpMethod.Post,
                RequestUri = new Uri(_URL + "messages")
            };

            var res = await _CLIENT.SendAsync(req);
            var result = await res.Content.ReadAsStringAsync();

            return result;
        }},

        {"delete", async (args)=>{
            var mismatch = argsCountError("delete", 1, args.Length);
            if (mismatch != null) {
                return mismatch;
            }

            var req = new HttpRequestMessage{
                Method = HttpMethod.Delete,
                RequestUri = new Uri(_URL + $"messages/{args[0]}")
            };

            var res = await _CLIENT.SendAsync(req);
            
            return await res.Content.ReadAsStringAsync();
        }},

        {"update", async (args)=>{
            var mismatch = argsCountError("update", 2, args.Length);
            if (mismatch != null) {
                return mismatch;
            }

            var req = new HttpRequestMessage{
                Content = JsonContent.Create(new {NewText = args[1]}),
                Method = HttpMethod.Patch,
                RequestUri = new Uri(_URL + $"messages/{args[0]}")
            };

            var res = await _CLIENT.SendAsync(req);
            
            return await res.Content.ReadAsStringAsync();
        }},
    };

    static async Task Main() {
        writeHelp();

        string? input;
        while (true) {
            Console.Write("Enter a command: ");
            input = Console.ReadLine();

            if (input == null || input == "help") {
                writeHelp();
                continue;
            }

            var parts = input.Split('|');

            if(!_COMMAND_DICT.ContainsKey(parts[0])) {
                Console.WriteLine($"\"{parts[0]}\" is not a command.");
                continue;
            }

            var res = await _COMMAND_DICT[parts[0]](parts.Skip(1).ToArray());

            var json = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(res), Formatting.Indented);

            Console.WriteLine(json);
        }
    }

    static void writeHelp() {
        Console.WriteLine(
            "Commands:\n" +
            "help                        display commands" +
            "all                         view all messages\n" +
            "get|[id]                    view one message\n" +
            "add|[Message]               add a message\n" +
            "delete|[id]                 delete a massage\n" +
            "update|[id]|[new message]   update a massage\n"
        );
    }

    static string? argsCountError(string cmd, int expected, int count) {
        if (expected != count) {
            return $"The \"{cmd}\" commend requires {expected} arguments";
        }

        return null;
    }
}