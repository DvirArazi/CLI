using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;

class Program {

    static HttpClient _CLIENT = new HttpClient();

    static Dictionary<string, Func<string[], Task<string>>> _COMMAND_DICT = new () {
        
        {"all", async (args)=>{
            var mismatch = argsCountError("all", 0, args.Length);
            if (mismatch != null) {
                return mismatch;
            }

            return await _CLIENT.GetStringAsync("https://localhost:7180/all");
        }},

        {"add", async (args)=>{
            var mismatch = argsCountError("add", 1, args.Length);
            if (mismatch != null) {
                return mismatch;
            }

            var json = JsonConvert.SerializeObject(new {Text = args[0]});
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _CLIENT.PostAsync("https://localhost:7180/add", data);
            var result = await res.Content.ReadAsStringAsync();

            return result;
        }},

        {"delete", async (args)=>{
            var mismatch = argsCountError("delete", 1, args.Length);
            if (mismatch != null) {
                return mismatch;
            }

            var req = new HttpRequestMessage{
                Content = JsonContent.Create(new {Text = args[0]}),
                Method = HttpMethod.Delete,
                RequestUri = new Uri("https://localhost:7180/delete")
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
                Content = JsonContent.Create(new {OldText = args[0], NewText = args[1]}),
                Method = HttpMethod.Patch,
                RequestUri = new Uri("https://localhost:7180/update")
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

            if (input == null) {
                writeHelp();
                continue;
            }

            var parts = input.Split(' ');

            if(!_COMMAND_DICT.ContainsKey(parts[0])) {
                Console.WriteLine($"\"{parts[0]}\" is not a command.");
                continue;
            }

            Console.WriteLine(await _COMMAND_DICT[parts[0]](parts.Skip(1).ToArray()) + '\n');
        }
    }

    static void writeHelp() {
        Console.WriteLine(
            "Commands:\n" + 
            "all                                view all messages\n" +
            "add [message]                      add a message\n" +
            "delete [message]                   delete a massage\n" +
            "update [message] [new message]     update a massage\n"
        );
    }

    static string? argsCountError(string cmd, int expected, int count) {
        if (expected != count) {
            return $"The \"{cmd}\" commend requires {expected} arguments";
        }

        return null;
    }
}