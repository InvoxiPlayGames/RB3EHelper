using DiscordRPC;
using System.Diagnostics.Metrics;
using System.Net.Sockets;
using System.Text;

namespace RB3EHelper
{
    public partial class MainForm : Form
    {
        UdpClient listener;
        DiscordRpcClient? discord = null;
        bool in_game = false;
        DateTime gamestarted;
        bool discord_active = false;
        byte platform = 0xFF;
        string rb3e_version = "";
        string song_artist = "";
        string song_name = "";
        string shortname = "";
        int band_member_count = 0;
        byte first_track_type = 0;
        byte first_difficulty = 0;

        private static string InstrumentName(byte track_type)
        {
            switch (track_type)
            {
                case 0: return "Drums";
                case 1: return "Guitar";
                case 2: return "Bass";
                case 3: return "Vocals";
                case 4: return "Keys";
                case 5: return "Pro Keys";
                case 6: return "Pro Guitar";
                case 7: return "Harmonies";
                case 8: return "Pro Bass";
            }
            return "Rock band";
        }

        private static string DifficultyName(byte difficulty)
        {
            switch (difficulty)
            {
                case 0: return "Easy";
                case 1: return "Medium";
                case 2: return "Hard";
                case 3: return "Expert";
            }
            return "Balls hard";
        }

        static private string PlatformString(byte platform)
        {
            switch (platform)
            {
                case 0:
                    return "Xbox 360";
                case 1:
                    return "Xenia";
                case 2:
                    return "Wii";
                case 3: // you guys have native RPC...
                    return "Dolphin*";
                default:
                    return "Unknown";
            }
        }

        public MainForm()
        {
            InitializeComponent();
            listener = new UdpClient(21070);
            listener.EnableBroadcast = true;
        }

        private void InvokeStringChange(Label label, string text)
        {
            Invoke(() =>
            {
                label.Text = text;
            });
        }

        private void InvokeDiscordChange()
        {
            Invoke(() =>
            {
                if (discord != null && discord_active)
                    discord.Invoke();
            });
        }

        private void UpdatePresence()
        {
            if (discord == null || discord_active == false)
                return;
            // set a default state
            RichPresence rp = new RichPresence()
            {
                Assets = new Assets()
                {
                    LargeImageKey = "rb3_box",
                    LargeImageText = "RB3Enhanced"
                }
            };

            // if we have the right info, show off the version
            if (platform != 0xFF && rb3e_version != "")
                rp.Assets.LargeImageText = $"RB3Enhanced {rb3e_version} on {PlatformString(platform)}";
            else if (platform != 0xFF) // otherwise just the platform
                rp.Assets.LargeImageText = $"RB3Enhanced on {PlatformString(platform)}";

            // set the song info if we're in a song
            if (in_game && song_name != "" && song_artist != "")
            {
                rp.Details = $"'{song_name}' by {song_artist}";
                if (band_member_count == 1)
                {
                    rp.State = $"Playing {DifficultyName(first_difficulty)} {InstrumentName(first_track_type)}";
                    rp.Assets.SmallImageKey = InstrumentName(first_track_type).ToLower();
                }
                else
                {
                    rp.State = $"In a {band_member_count} player band";
                    rp.Assets.SmallImageKey = "band";
                }
                rp.Timestamps = new Timestamps()
                {
                    Start = gamestarted
                };
            }
            else // sometimes this gets stuck
                rp.Details = "In the menus";

            discord.SetPresence(rp);
            InvokeDiscordChange();
        }

        private void WriteFiles()
        {
            if (txttargetBox.Checked)
            {
                try
                {
                    File.WriteAllText("song_name.txt", song_name);
                    File.WriteAllText("song_artist.txt", song_artist);
                    File.WriteAllText("song.txt", song_name + Environment.NewLine + song_artist);
                } catch (IOException)
                {
                    // i dont know
                }
            }
        }

        async private void ListenerThread()
        {
            while (true)
            {
                UdpReceiveResult result = await listener.ReceiveAsync();
                Console.WriteLine($"Got {result.Buffer.Length} bytes from {result.RemoteEndPoint}");
                if (result.Buffer.Length >= RB3E_EventHeader.Size)
                {
                    RB3E_EventHeader header = new RB3E_EventHeader(result.Buffer, 0);
                    if (header.ProtocolMagic != 0x52423345 || header.ProtocolVersion != 0)
                    {
                        Console.WriteLine("Ignoring packet with wrong magic/version.");
                        continue;
                    }
                    platform = header.Platform;
                    InvokeStringChange(statusLabel, $"{PlatformString(platform)} @ {result.RemoteEndPoint.Address} ({(in_game ? "in-game" : "menus")})");
                    switch (header.PacketType)
                    {
                        case 0:
                            rb3e_version = Encoding.ASCII.GetString(result.Buffer, RB3E_EventHeader.Size, header.PacketSize);
                            InvokeStringChange(versionLabel, rb3e_version);
                            break;
                        case 1:
                            in_game = result.Buffer[RB3E_EventHeader.Size] >= 0x01;
                            if (!in_game)
                            {
                                song_name = "";
                                song_artist = "";
                                shortname = "";
                                band_member_count = 0;
                                InvokeStringChange(songnameLabel, "N/A");
                                InvokeStringChange(songartistLabel, "N/A");
                                InvokeStringChange(shortnameLabel, "N/A");
                            } else
                            {
                                gamestarted = DateTime.UtcNow;
                            }
                            WriteFiles();
                            UpdatePresence();
                            break;
                        case 2:
                            song_name = Encoding.ASCII.GetString(result.Buffer, RB3E_EventHeader.Size, header.PacketSize);
                            InvokeStringChange(songnameLabel, song_name);
                            break;
                        case 3:
                            song_artist = Encoding.ASCII.GetString(result.Buffer, RB3E_EventHeader.Size, header.PacketSize);
                            InvokeStringChange(songartistLabel, song_artist);
                            break;
                        case 4:
                            shortname = Encoding.ASCII.GetString(result.Buffer, RB3E_EventHeader.Size, header.PacketSize);
                            InvokeStringChange(shortnameLabel, shortname);
                            break;
                        case 7:
                            RB3E_EventBandInfo bandinfo = new RB3E_EventBandInfo(result.Buffer, RB3E_EventHeader.Size);
                            for (int i = 0; i < 4; i++)
                            {
                                if (band_member_count == 0)
                                {
                                    first_difficulty = bandinfo.Difficulty[i];
                                    first_track_type = bandinfo.TrackType[i];
                                }
                                if (bandinfo.MemberExists[i] == 0x01)
                                    band_member_count++;
                            }
                            break;
                        default:
                            Console.WriteLine($"Unknown packet type {header.PacketType}");
                            break;
                    }
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            rpctargetBox.Checked = Properties.Settings.Default.DiscordOutput;
            txttargetBox.Checked = Properties.Settings.Default.TXTOutput;
            Task.Run(ListenerThread);
        }

        private void rpctargetBox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.DiscordOutput = rpctargetBox.Checked;
            Properties.Settings.Default.Save();
            if (rpctargetBox.Checked)
            {
                if (discord == null)
                {
                    discord = new DiscordRpcClient("923035110062841887", -1, null, false, null);
                    discord_active = discord.Initialize();
                    if (discord_active)
                        UpdatePresence();
                }
            } else
            {
                if (discord != null)
                {
                    discord_active = false;
                    discord.Dispose();
                    discord = null;
                }
            }
        }

        private void txttargetBox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.TXTOutput = txttargetBox.Checked;
            Properties.Settings.Default.Save();
        }
    }
}