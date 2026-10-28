using System;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using System.Diagnostics;
using System.Threading.Tasks;

public class TwitchReader
{
    private TwitchClient client;
    private readonly string channel = "JacobCereal1"; // Replace with your own twitch name
    private string oauthToken = Environment.GetEnvironmentVariable("TWITCH_OAUTH"); // Set your twitch token in environment variables to not expose it by accident (Would be bad)
    public void Start()
    {
        var credentials = new ConnectionCredentials(
            "CerealBot1", // Replace with your own custom name, (Does not show up in chat like this btw)
            $"oauth:{oauthToken}"
        );

        client = new TwitchClient();
        client.Initialize(credentials, channel);

        // Read messages, parse information to call functions (Hopefully works)
        client.OnMessageReceived += async (sender, e) =>
        {
            Console.WriteLine(
                $"[{e.ChatMessage.Username}: {e.ChatMessage.Message}]"
            );
            
            if (!e.ChatMessage.IsBroadcaster)
            {
                if (e.ChatMessage.Message.Contains("inv"))
                {
                    string[] words = e.ChatMessage.Message.Split(" ");
                    Console.WriteLine($"{words[0]}, {words[1]}");
                    await PressKey(words[1]);
                }
                else
                {
                    switch (e.ChatMessage.Message.ToLowerInvariant())
                    {
                        case "forward":
                            await PressKey("W");
                            break;
                        case "left":
                            await PressKey("A");
                            break;
                        case "back":
                            await PressKey("S");
                            break;
                        case "right":
                            await PressKey("D");
                            break;
                        case "jump":
                            await PressKey("Space");
                            break;
                        case "shoot":
                            await MouseClick("Left");
                            break;
                        case "longshoot":
                            await MouseClick("Left", 500);
                            break;
                        case "aimup":
                            await AimMouse('Y', "Up");
                            break;
                        case "aimright":
                            await AimMouse('X', "Right");
                            break;
                        case "aimdown":
                            await AimMouse('Y', "Down");
                            break;
                        case "aimleft":
                            await AimMouse('X', "Left");
                            break;
                        default:
                            return;
                    }
                }
            }
        };

        // Join chat once connected
        client.OnConnected += (sender, e) =>
        {
            Console.WriteLine($"Connected to Channel:{channel}!");

            client.JoinChannel(channel);

            SendMessage("Hello chat! The bot is online.");
        };

        client.OnConnectionError += (sender, e) =>
        {
            Console.WriteLine($"Connection error: {e.Error.Message}");
        };

        client.Connect();
    }

    /// <summary>
    /// Sends a message to connected Twitch Chat
    /// </summary>
    /// <param name="message"></param>
    public void SendMessage(string message)
    {
        if (client != null && client.IsConnected)
        {
            client.SendMessage(channel, message);
        }
    }
    
    /// <summary>
    /// Simulates a keypress on a keyboard
    /// </summary>
    /// <param name="key"></param>
    private async Task PressKey(string key)
    {
        var dict = new Dictionary<string, int>
        {
            ["W"] = 17,
            ["A"] = 30,
            ["S"] = 31,
            ["D"] = 32,
            ["Space"] = 57,
            // Number Keys for Inventory slots
            ["1"] = 2,
            ["2"] = 3,
            ["3"] = 4,
            ["4"] = 5,
            ["5"] = 6,
            ["6"] = 7,
            ["7"] = 8,
            ["8"] = 9,
            ["9"] = 10,
            // Optional, Some games won't do more than 9
            ["10"] = 11
        };
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "/usr/bin/ydotool",
            Arguments = $"key --key-delay 100 {dict[key]}:1 {dict[key]}:0",
            UseShellExecute = false,
            RedirectStandardError = true
        });

        if (process == null)
        {
            Console.WriteLine("Failed to start ydotool.");
            return;
        }

        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"ydotool exited with code {process.ExitCode}: {error}");
        }
    }
    
    /// <summary>
    /// Simulates a mouse click
    /// </summary>
    private async Task MouseClick(string mouseBtn, int clickTime = 1)
    {
        var dict = new Dictionary<string, ( string Down, string Up )>
        {
            ["Left"] = ("0x80", "0x40"),
            ["Right"] = ("0x81", "0x41")
        };
        
        var (upValue, downValue) = dict[mouseBtn];
            
        var down = Process.Start(new ProcessStartInfo
        {
            FileName = "/usr/bin/ydotool", 
            Arguments = $"click --down {downValue}",
            UseShellExecute = false,
            RedirectStandardError = true
        });
        Console.WriteLine(downValue);
        await down.WaitForExitAsync();
         
        await Task.Delay(clickTime);

        var up = Process.Start(new ProcessStartInfo
        {
            FileName = "/usr/bin/ydotool",
            Arguments = $"click --up {upValue}",
            UseShellExecute = false,
            RedirectStandardError = true
        });
        Console.WriteLine(upValue);
        await up.WaitForExitAsync();
    }
   
    /// <summary>
    /// Simulates mouse movement relative to current position
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="aimDirection"></param>
    private async Task AimMouse(char direction, string aimDirection)
    {
        var dict = new Dictionary<string, int>()
        {
            ["Up"] = -100,
            ["Down"] = 100,
            ["Left"] = -100,
            ["Right"] = 100
        };
        
        if (Char.ToUpper(direction) == 'Y')
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "/usr/bin/ydotool",
                Arguments = $"mousemove -x 0 -y {dict[aimDirection]}",
                UseShellExecute = false,
                RedirectStandardError = true
            });
            await process.WaitForExitAsync();
        }
        else
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "/usr/bin/ydotool",
                Arguments = $"mousemove -x {dict[aimDirection]} -y 0",
                UseShellExecute = false,
                RedirectStandardError = true
            });
            await process.WaitForExitAsync(); 
        }
    }
}