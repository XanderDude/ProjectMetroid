using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

public partial class SoundFriend : Node
{
    
    public static Dictionary<string, Godot.AudioStreamPlayer> sounds = null;

    public override void _Ready()
    {
       sounds = new Dictionary<string, Godot.AudioStreamPlayer>();
       
       foreach (Node child in GetChildren())
        {
            string childName = child.Name.ToString();
            //GD.Print("SoundFriend: Loaded sound - " + childName);
            
                sounds[childName] = child as Godot.AudioStreamPlayer;


        }
       
    }


    //PROOF OF CONCEPT
    /*public void change_background_music(string area)
    {
        var musicPlayer = GetNode<Godot.AudioStreamPlayer>("background_music");
        var newStream = new AudioStream();
        if (area == "sewer")
        {
            newStream = ResourceLoader.Load<Godot.AudioStream>("res://Sounds/Music/Sewer.mp3");
            musicPlayer.Stream = newStream;
            musicPlayer.Play();
        }
        else
        {
            GD.PrintErr("SoundFriend: Music stream '" + area + "' not found!");
        }
    }
    */
//-----------------------------------
    public static void Play(string soundName)
    {
        if (sounds.ContainsKey(soundName))
        {
            sounds[soundName].Play();
        }
        else
        {
            GD.PrintErr($"SoundFriend: Sound '{soundName}' not found!");
        }

        
    }

    public static void Stop(string soundName)
    {
        if (sounds.ContainsKey(soundName))
        {
            sounds[soundName].Stop();
        }
        else
        {
            GD.PrintErr($"SoundFriend: Sound '{soundName}' not found!");
        }
    }



    
}
