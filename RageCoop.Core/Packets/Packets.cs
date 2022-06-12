﻿using System;
using System.Collections.Generic;
using System.Text;
using Lidgren.Network;
using Newtonsoft.Json;
using GTA.Math;

namespace RageCoop.Core
{
    /*
    /// <summary>
    /// 
    /// </summary>
    public struct LVector3
    {
        #region CLIENT-ONLY
        /// <summary>
        /// 
        /// </summary>
        public Vector3 ToVector()
        {
            return new Vector3(X, Y, Z);
        }
        #endregion
        #region SERVER-ONLY
        public float Length() => (float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
        public static LVector3 Subtract(LVector3 pos1, LVector3 pos2) { return new LVector3(pos1.X - pos2.X, pos1.Y - pos2.Y, pos1.Z - pos2.Z); }
        public static bool Equals(LVector3 value1, LVector3 value2) => value1.X == value2.X && value1.Y == value2.Y && value1.Z == value2.Z;
        public static LVector3 operator /(LVector3 value1, float value2)
        {
            float num = 1f / value2;
            return new LVector3(value1.X * num, value1.Y * num, value1.Z * num);
        }
        public static LVector3 operator -(LVector3 left, LVector3 right)
        {
            return new LVector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }
        public static LVector3 operator -(LVector3 value)
        {
            return default(LVector3) - value;
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        public LVector3(float X, float Y, float Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        /// <summary>
        /// 
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Z { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct LQuaternion
    {
        #region CLIENT-ONLY
        /// <summary>
        /// 
        /// </summary>
        public Quaternion ToQuaternion()
        {
            return new Quaternion(X, Y, Z, W);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public LQuaternion(float X, float Y, float Z, float W)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.W = W;
        }

        /// <summary>
        /// 
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Z { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float W { get; set; }
    }
    */
    public enum PacketTypes:byte
    {
        Handshake=0,
        PlayerConnect=1,
        PlayerDisconnect=2,
        PlayerInfoUpdate=3,
        
        ChatMessage=10,
        NativeCall=11,
        NativeResponse=12,
        //Mod=13,
        CleanUpWorld=14,
       
        FileTransferChunk=15,
        FileTransferRequest=16,
        FileTransferComplete=17,
        
        CustomEvent = 18,
        InvokeCustomEvent=19,
        #region Sync

        #region INTERVAL
        VehicleSync = 20,
        VehicleStateSync = 21,
        PedSync = 22,
        PedStateSync = 23,
        ProjectileSync=24,
        #endregion

        #region EVENT

        PedKilled=30,
        BulletShot=31,
        EnteringVehicle=32,
        LeaveVehicle = 33,
        EnteredVehicle=34,
        OwnerChanged=35,
        VehicleBulletShot = 36,
        NozzleTransform=37,

        #endregion

        #endregion
    }
    public static class PacketExtensions
    {
        public static bool IsSyncEvent(this PacketTypes p)
        {
            return (30<=(byte)p)&&((byte)p<=40);
        }
    }

    public enum ConnectionChannel
    {
        Default = 0,
        Chat = 5,
        Native = 6,
        Mod = 7,
        File = 8,
        Event = 9,
        VehicleSync=20,
        PedSync=21,
        ProjectileSync = 22,
        SyncEvents =30,
    }

    [Flags]
    public enum PedDataFlags:ushort
    {
        None=0,
        IsAiming = 1 << 0,
        IsReloading = 1 << 2,
        IsJumping = 1 << 3,
        IsRagdoll = 1 << 4,
        IsOnFire = 1 << 5,
        IsInParachuteFreeFall = 1 << 6,
        IsParachuteOpen = 1 << 7,
        IsOnLadder = 1 << 8,
        IsVaulting = 1 << 9,
        IsInCover=1<< 10,
    }

    #region ===== VEHICLE DATA =====
    public enum VehicleDataFlags:ushort
    {
        IsEngineRunning = 1 << 0,
        AreLightsOn = 1 << 1,
        AreBrakeLightsOn = 1 << 2,
        AreHighBeamsOn = 1 << 3,
        IsSirenActive = 1 << 4,
        IsDead = 1 << 5,
        IsHornActive = 1 << 6,
        IsTransformed = 1 << 7,
        RoofOpened = 1 << 8,
        OnTurretSeat = 1 << 9,
        IsAircraft = 1 << 10,
        IsDeluxoHovering=1 << 11, 
    }
    
    
    public struct VehicleDamageModel
    {
        public byte BrokenDoors { get; set; }
        public byte OpenedDoors { get; set; }
        public byte BrokenWindows { get; set; }
        public short BurstedTires { get; set; }
        public byte LeftHeadLightBroken { get; set; }
        public byte RightHeadLightBroken { get; set; }
    }
    #endregion

    interface IPacket
    {
        void Pack(NetOutgoingMessage message);
        void Unpack(byte[] array);
    }

    public abstract class Packet : IPacket
    {
        public abstract void Pack(NetOutgoingMessage message);
        public abstract void Unpack(byte[] array);
    }

    public partial class Packets
    {

        public class ChatMessage : Packet
        {
            public string Username { get; set; }

            public string Message { get; set; }

            public override void Pack(NetOutgoingMessage message)
            {
                #region PacketToNetOutGoingMessage
                message.Write((byte)PacketTypes.ChatMessage);

                List<byte> byteArray = new List<byte>();

                byte[] usernameBytes = Encoding.UTF8.GetBytes(Username);
                byte[] messageBytes = Encoding.UTF8.GetBytes(Message);

                // Write UsernameLength
                byteArray.AddRange(BitConverter.GetBytes(usernameBytes.Length));

                // Write Username
                byteArray.AddRange(usernameBytes);

                // Write MessageLength
                byteArray.AddRange(BitConverter.GetBytes(messageBytes.Length));

                // Write Message
                byteArray.AddRange(messageBytes);

                byte[] result = byteArray.ToArray();

                message.Write(result.Length);
                message.Write(result);
                #endregion
            }

            public override void Unpack(byte[] array)
            {
                #region NetIncomingMessageToPacket
                BitReader reader = new BitReader(array);

                // Read username
                int usernameLength = reader.ReadInt();
                Username = reader.ReadString(usernameLength);

                // Read message
                int messageLength = reader.ReadInt();
                Message = reader.ReadString(messageLength);
                #endregion
            }
        }

        public class CustomEvent : Packet
        {
            public int Hash { get; set; }
            public byte[] Data { get; set; }

            public override void Pack(NetOutgoingMessage message)
            {
                message.Write((byte)PacketTypes.CustomEvent);

                List<byte> byteArray = new List<byte>();


                // Write hash
                byteArray.AddInt(Hash);

                // Write data
                byteArray.AddRange(Data);

                byte[] result = byteArray.ToArray();

                message.Write(result.Length);
                message.Write(result);
            }

            public override void Unpack(byte[] array)
            {
                BitReader reader = new BitReader(array);

                Hash = reader.ReadInt();

                Data=reader.ReadByteArray(array.Length-4);
            }
        }
        public class InvokeCustomEvent : Packet
        {
            public int Hash { get; set; }
            public int[] Targets { get; set; }
            public byte[] Data { get; set; }

            public override void Pack(NetOutgoingMessage message)
            {
                message.Write((byte)PacketTypes.InvokeCustomEvent);

                List<byte> byteArray = new List<byte>();


                // Write hash
                byteArray.AddInt(Hash);

                // Write targets
                byteArray.AddInt(Targets.Length);
                foreach (var target in Targets)
                {
                    byteArray.AddInt(target);
                }

                // Write data
                byteArray.AddRange(Data);

                byte[] result = byteArray.ToArray();

                message.Write(result.Length);
                message.Write(result);
            }

            public override void Unpack(byte[] array)
            {
                BitReader reader = new BitReader(array);

                Hash = reader.ReadInt();

                Targets = new int[reader.ReadInt()];
                for(int i = 0; i < Targets.Length; i++)
                {
                    Targets[i]=reader.ReadInt();
                }

                Data=reader.ReadByteArray(array.Length-4);
            }
        }
        #region ===== NATIVECALL =====
        public class NativeCall : Packet
        {
            public ulong Hash { get; set; }

            public List<object> Args { get; set; }

            public override void Pack(NetOutgoingMessage message)
            {
                #region PacketToNetOutGoingMessage
                message.Write((byte)PacketTypes.NativeCall);

                List<byte> byteArray = new List<byte>();

                // Write Hash
                byteArray.AddRange(BitConverter.GetBytes(Hash));

                // Write Args
                byteArray.AddRange(BitConverter.GetBytes(Args.Count));
                Args.ForEach(x =>
                {
                    Type type = x.GetType();

                    if (type == typeof(int))
                    {
                        byteArray.Add(0x00);
                        byteArray.AddRange(BitConverter.GetBytes((int)x));
                    }
                    else if (type == typeof(bool))
                    {
                        byteArray.Add(0x01);
                        byteArray.AddRange(BitConverter.GetBytes((bool)x));
                    }
                    else if (type == typeof(float))
                    {
                        byteArray.Add(0x02);
                        byteArray.AddRange(BitConverter.GetBytes((float)x));
                    }
                    else if (type == typeof(string))
                    {
                        byteArray.Add(0x03);
                        byte[] stringBytes = Encoding.UTF8.GetBytes((string)x);
                        byteArray.AddRange(BitConverter.GetBytes(stringBytes.Length));
                        byteArray.AddRange(stringBytes);
                    }
                    else if (type == typeof(Vector3))
                    {
                        byteArray.Add(0x04);
                        Vector3 vector = (Vector3)x;
                        byteArray.AddRange(BitConverter.GetBytes(vector.X));
                        byteArray.AddRange(BitConverter.GetBytes(vector.Y));
                        byteArray.AddRange(BitConverter.GetBytes(vector.Z));
                    }
                });

                byte[] result = byteArray.ToArray();

                message.Write(result.Length);
                message.Write(result);
                #endregion
            }

            public override void Unpack(byte[] array)
            {
                #region NetIncomingMessageToPacket
                BitReader reader = new BitReader(array);

                // Read Hash
                Hash = reader.ReadULong();

                // Read Args
                Args = new List<object>();
                int argsLength = reader.ReadInt();
                for (int i = 0; i < argsLength; i++)
                {
                    byte argType = reader.ReadByte();
                    switch (argType)
                    {
                        case 0x00:
                            Args.Add(reader.ReadInt());
                            break;
                        case 0x01:
                            Args.Add(reader.ReadBool());
                            break;
                        case 0x02:
                            Args.Add(reader.ReadFloat());
                            break;
                        case 0x03:
                            int stringLength = reader.ReadInt();
                            Args.Add(reader.ReadString(stringLength));
                            break;
                        case 0x04:
                            Args.Add(new Vector3()
                            {
                                X = reader.ReadFloat(),
                                Y = reader.ReadFloat(),
                                Z = reader.ReadFloat()
                            });
                            break;
                    }
                }
                #endregion
            }
        }

        public class NativeResponse : Packet
        {
            public ulong Hash { get; set; }

            public List<object> Args { get; set; }

            public byte? ResultType { get; set; }

            public long ID { get; set; }

            public override void Pack(NetOutgoingMessage message)
            {
                #region PacketToNetOutGoingMessage
                message.Write((byte)PacketTypes.NativeResponse);

                List<byte> byteArray = new List<byte>();

                // Write Hash
                byteArray.AddRange(BitConverter.GetBytes(Hash));

                Type type;

                // Write Args
                byteArray.AddRange(BitConverter.GetBytes(Args.Count));
                Args.ForEach(x =>
                {
                    type = x.GetType();

                    if (type == typeof(int))
                    {
                        byteArray.Add(0x00);
                        byteArray.AddRange(BitConverter.GetBytes((int)x));
                    }
                    else if (type == typeof(bool))
                    {
                        byteArray.Add(0x01);
                        byteArray.AddRange(BitConverter.GetBytes((bool)x));
                    }
                    else if (type == typeof(float))
                    {
                        byteArray.Add(0x02);
                        byteArray.AddRange(BitConverter.GetBytes((float)x));
                    }
                    else if (type == typeof(string))
                    {
                        byteArray.Add(0x03);
                        byte[] stringBytes = Encoding.UTF8.GetBytes((string)x);
                        byteArray.AddRange(BitConverter.GetBytes(stringBytes.Length));
                        byteArray.AddRange(stringBytes);
                    }
                    else if (type == typeof(Vector3))
                    {
                        byteArray.Add(0x04);
                        Vector3 vector = (Vector3)x;
                        byteArray.AddRange(BitConverter.GetBytes(vector.X));
                        byteArray.AddRange(BitConverter.GetBytes(vector.Y));
                        byteArray.AddRange(BitConverter.GetBytes(vector.Z));
                    }
                });

                byteArray.AddRange(BitConverter.GetBytes(ID));

                // Write type of result
                if (ResultType.HasValue)
                {
                    byteArray.Add(ResultType.Value);
                }

                byte[] result = byteArray.ToArray();

                message.Write(result.Length);
                message.Write(result);
                #endregion
            }

            public override void Unpack(byte[] array)
            {
                #region NetIncomingMessageToPacket
                BitReader reader = new BitReader(array);

                // Read Hash
                Hash = reader.ReadULong();

                // Read Args
                Args = new List<object>();
                int argsLength = reader.ReadInt();
                for (int i = 0; i < argsLength; i++)
                {
                    byte argType = reader.ReadByte();
                    switch (argType)
                    {
                        case 0x00:
                            Args.Add(reader.ReadInt());
                            break;
                        case 0x01:
                            Args.Add(reader.ReadBool());
                            break;
                        case 0x02:
                            Args.Add(reader.ReadFloat());
                            break;
                        case 0x03:
                            int stringLength = reader.ReadInt();
                            Args.Add(reader.ReadString(stringLength));
                            break;
                        case 0x04:
                            Args.Add(new Vector3()
                            {
                                X = reader.ReadFloat(),
                                Y = reader.ReadFloat(),
                                Z = reader.ReadFloat()
                            });
                            break;
                    }
                }

                ID = reader.ReadLong();

                // Read type of result
                if (reader.CanRead(1))
                {
                    ResultType = reader.ReadByte();
                }
                #endregion
            }
        }
        #endregion // ===== NATIVECALL =====
    }

    public static class CoopSerializer
    {
        /// <summary>
        /// ?
        /// </summary>
        public static byte[] Serialize(this object obj)
        {
            if (obj == null)
            {
                return null;
            }

            string jsonString = JsonConvert.SerializeObject(obj);
            return System.Text.Encoding.UTF8.GetBytes(jsonString);
        }

        /// <summary>
        /// ?
        /// </summary>
        public static T Deserialize<T>(this byte[] bytes) where T : class
        {
            if (bytes == null)
            {
                return null;
            }

            var jsonString = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}