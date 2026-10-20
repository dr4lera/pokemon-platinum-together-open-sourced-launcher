using System.Buffers.Binary;
using System.Text.Json;
using System.IO.Pipes;
using PlatinumPrivateLauncher;

if(args.Contains("--live")) {
 var accepted=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
 try { await using(var presence=new DiscordPresence(()=>"Testing private launcher",s=>{Console.WriteLine(s);if(s.Contains("accepted"))accepted.TrySetResult();})) {
  await accepted.Task.WaitAsync(TimeSpan.FromSeconds(25));await Task.Delay(1500);
 }} catch(TimeoutException) {Console.WriteLine("FAIL: Discord did not accept the invite subscription and presence.");Environment.ExitCode=1;return;}
 Console.WriteLine("PASS: live Discord accepted SET_ACTIVITY; cleanup attempted.");return;
}
using var token=new CancellationTokenSource(3000);
using var memory=new MemoryStream();
await DiscordPresence.Write(memory,3,new byte[]{1,2,3},token.Token);
memory.Position=0;var result=await DiscordPresence.Read(memory,token.Token);
if(result.Opcode!=3||!result.Body.SequenceEqual(new byte[]{1,2,3}))throw new Exception("Frame roundtrip");
var header=new byte[8];BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(4),1048577);
try{await DiscordPresence.Read(new MemoryStream(header),token.Token);throw new Exception("Oversized frame accepted");}catch(IOException){}
try{await DiscordPresence.Read(new MemoryStream(new byte[]{1}),token.Token);throw new Exception("Truncated header accepted");}catch(EndOfStreamException){}
var activity=JsonSerializer.SerializeToElement(DiscordPresence.Activity("In the launcher","logo",123));
if(activity.GetProperty("buttons").GetArrayLength()!=2||activity.GetProperty("assets").GetProperty("large_image").GetString()!="logo")throw new Exception("Activity payload");
if(activity.ToString().Contains("seed")||activity.ToString().Contains("token"))throw new Exception("Private metadata");
Console.WriteLine("PASS: IPC framing, size rejection, truncation rejection, activity links and asset.");

const string secret="https://world.example.com/private/j/ABCDEFGHIJKLMNOPQRSTUV";
var partyActivity=JsonSerializer.SerializeToElement(DiscordPresence.Activity("Hosting","logo",123,new DiscordParty("party-id",2,4,secret)));
if(partyActivity.GetProperty("secrets").GetProperty("join").GetString()!=secret||partyActivity.GetProperty("party").GetProperty("size")[1].GetInt32()!=4)throw new Exception("Joinable party payload");
foreach(var bad in new[]{"http://world.example.com/private/j/ABCDEFGHIJKLMNOPQRSTUV","https://localhost/private/j/ABCDEFGHIJKLMNOPQRSTUV","https://user:pass@world.example.com/private/j/ABCDEFGHIJKLMNOPQRSTUV",secret+"?token=bad",secret+"/extra","file:///C:/game"})
 if(DiscordJoinAddress.Valid(bad))throw new Exception("Untrusted invite accepted: "+bad);
using var wrong=JsonDocument.Parse("{\"cmd\":\"DISPATCH\",\"evt\":\"ACTIVITY_SPECTATE\",\"data\":{\"secret\":\""+secret+"\"}}");
if(DiscordPresence.JoinSecret(wrong.RootElement)!=null)throw new Exception("Wrong event accepted");

// A local fake Discord peer tests the actual duplex reader while commands await acknowledgements.
using var deadline=new CancellationTokenSource(6000);
var pipeName="pt-discord-test-"+Guid.NewGuid().ToString("N");
using var server=new NamedPipeServerStream(pipeName,PipeDirection.InOut,1,PipeTransmissionMode.Byte,PipeOptions.Asynchronous);
using var client=new NamedPipeClientStream(".",pipeName,PipeDirection.InOut,PipeOptions.Asynchronous);
var connecting=client.ConnectAsync(deadline.Token);await server.WaitForConnectionAsync(deadline.Token);await connecting;
var received=new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
var connection=new DiscordPresence.Connection(client,s=>received.TrySetResult(s));
var reader=connection.Receive(deadline.Token);
var peer=Task.Run(async()=>{
 var subscribe=await DiscordPresence.Read(server,deadline.Token);using var cmd=JsonDocument.Parse(subscribe.Body);
 if(cmd.RootElement.GetProperty("cmd").GetString()!="SUBSCRIBE"||cmd.RootElement.GetProperty("evt").GetString()!="ACTIVITY_JOIN")throw new Exception("Subscription payload");
 await DiscordPresence.Write(server,1,JsonSerializer.SerializeToUtf8Bytes(new{cmd="DISPATCH",evt="ACTIVITY_JOIN",data=new{secret}}),deadline.Token);
 await DiscordPresence.Write(server,3,new byte[]{9,8,7},deadline.Token);
 var pong=await DiscordPresence.Read(server,deadline.Token);if(pong.Opcode!=4||!pong.Body.SequenceEqual(new byte[]{9,8,7}))throw new Exception("Ping response");
 await DiscordPresence.Write(server,1,JsonSerializer.SerializeToUtf8Bytes(new{cmd="SUBSCRIBE",evt="ACTIVITY_JOIN",nonce=cmd.RootElement.GetProperty("nonce").GetString()}),deadline.Token);
},deadline.Token);
await connection.Command("SUBSCRIBE",null,"ACTIVITY_JOIN",deadline.Token);await peer;
if(await received.Task.WaitAsync(deadline.Token)!=secret)throw new Exception("Join event lost while awaiting acknowledgement");
deadline.Cancel();try{await reader;}catch(OperationCanceledException){}
Console.WriteLine("PASS: party secrets, URL rejection, event dispatch during command ACK, subscription and ping/pong.");
