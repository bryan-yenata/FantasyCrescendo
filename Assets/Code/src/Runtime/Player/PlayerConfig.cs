using HouraiTeahouse.FantasyCrescendo.Networking;
using System;
using System.Linq;

namespace HouraiTeahouse.FantasyCrescendo {

/// <summary>
/// A data object for configuring a single player within a multiplayer match.
/// </summary>
[Serializable]
public struct PlayerConfig : IValidatable, INetworkSerializable {

  /// <summary>
  /// The Player ID of the player. Determines what is visually displayed
  /// to denote the player.
  /// </summary>
  public byte PlayerID;

  /// <summary>
  /// The local player number. Mainly used to determine what local input 
  /// device to read the input from.
  /// </summary>
  public sbyte LocalPlayerID;
  public PlayerSelection Selection;

  public bool IsLocal => LocalPlayerID >= 0;
  public bool IsValid => Selection.IsValid;

  public void Serialize(Serializer serializer) {
    serializer.Write(PlayerID);
    serializer.Write(LocalPlayerID);
    serializer.Write(Selection);
  }

  public void Deserialize(Deserializer deserializer) {
    PlayerID = deserializer.ReadByte();
    LocalPlayerID = deserializer.ReadSByte();
    Selection = deserializer.Read<PlayerSelection>();
  }

  public override string ToString() => Selection.ToString();

}

/// <summary>
/// A data object for managing the human selected elements of a player's
/// configuration.
/// </summary>
[Serializable]
public struct PlayerSelection : IValidatable, INetworkSerializable {

  public uint CharacterID;            // 1-4 bytes
  public byte Pallete;                // 1 byte
  
  public bool IsValid {
    get {
      var character = Registry.Get<CharacterData>().Get(CharacterID);
      if (character == null) return false;
      return Pallete < character.Portraits.Count;
    }
  }

  public void Serialize(Serializer serializer) {
    serializer.Write(CharacterID);
    serializer.Write(Pallete);
  }

  public void Deserialize(Deserializer deserializer) {
    CharacterID = deserializer.ReadUInt32();
    Pallete = deserializer.ReadByte();
  }

  public CharacterData GetCharacter() => Registry.Get<CharacterData>().Get(CharacterID);

  public override string ToString() => $"Selection({CharacterID},{Pallete})";

}

}