// Reference£∫MMD4Unity Tools / MMD4Mecainm / MMD Encode And So On
// So Thanks To This Author.From PowerBear!
// The Parase Used The File Stream
// PowerBear£®ﬂŸ¡®ﬂŸ¡®£∫¥Û«ß–°–‹£©

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


namespace PowerBearEditor.MMD {
    public class ConvertVMD {
        public static VMDFile Read(Stream stream) {
            VMDFile output = new();
            using var reader = new BinaryReader(stream);
            // byte*30 VersionInformation
            string header = ReadByBytesToString(reader, 30);
            if (header == "Vocaloid Motion Data 0002") {
                output.HeaderType = VMDHeaderType.VocaloidMotionData0002;
            } else {
                output.HeaderType = VMDHeaderType.VocaloidMotionDatafile;
            }

            // byte*(10/20) Model Name
            string modelName = ReadByBytesToString(reader, output.HeaderType == VMDHeaderType.VocaloidMotionDatafile ? 10 : 20);
            output.ModelName = modelName;

            // BoneKeyFrameNumber (Do Not Need) And Read To End
            UInt32 boneFrameCount = reader.ReadUInt32();
            ReadByBytesToString(reader, (int)boneFrameCount * 111);

            // MorphFrameCount
            UInt32 morphFrameCount = reader.ReadUInt32();
            for (int i = 0; i < morphFrameCount; i++) {
                output.MorphFrames.Add(new() {
                    MorphName = ReadByBytesToString(reader, 15),
                    FrameTime = reader.ReadUInt32(),
                    Weight = reader.ReadSingle()
                });
            }

            // CameraCount
            UInt32 cameraFrameCount = reader.ReadUInt32();
            for (int i = 0; i < cameraFrameCount; i++) {
                output.CameraFrames.Add(new() { 
                    FrameIndex = reader.ReadUInt32(),
                    Distance = reader.ReadSingle(),
                    XPosition = reader.ReadSingle(),
                    YPosition = reader.ReadSingle(),
                    ZPosition = reader.ReadSingle(),
                    XRotation = reader.ReadSingle(),
                    YRotation = reader.ReadSingle(),
                    ZRotation = reader.ReadSingle(),
                    Curve = new() {
                        AX = reader.ReadByte(),
                        AY = reader.ReadByte(),
                        BX = reader.ReadByte(),
                        BY = GetCameraByte(reader)
                    },
                    FOV = reader.ReadUInt32(),
                    Orthographic = reader.ReadBoolean()
                });
            }

            reader.Dispose();
            return output;
        }
        private static string ReadByBytesToString(BinaryReader reader, int count) {
            var encoder = Encoding.GetEncoding("shift_jis");
            var output = encoder.GetString(reader.ReadBytes(count)).Trim('\0');
            return output;
        }

        private static byte GetCameraByte(BinaryReader reader) {
            var bt = reader.ReadByte();
            reader.ReadBytes(20);
            return bt;
        }
    }

    public class VMDFile {
        public string ModelName { get; set; }
        public VMDHeaderType HeaderType { get; set; }
        public List<MorphKeyFrame> MorphFrames { get; set; } = new();
        public List<CameraFrame> CameraFrames { get; set; } = new();
    }

    public enum VMDHeaderType {
        VocaloidMotionDatafile,
        VocaloidMotionData0002,
    }

    public class MorphKeyFrame {
        public string MorphName { get; set; }
        public UInt32 FrameTime { get; set; }
        public float Weight { get; set; }
    }

    public class CameraFrame {
        public uint FrameIndex { get; set; }
        public float Distance { get; set; }
        /// <summary>
        /// Notice That MMD Use Right Hand Axies btw Unity Use Left Hand Axies.
        /// The X in MMD means Z in Unity.
        /// </summary>
        public float XPosition { get; set; }
        public float YPosition { get; set; }
        /// <summary>
        /// Notice That MMD Use Right Hand Axies btw Unity Use Left Hand Axies.
        /// The Z in MMD means X in Unity.
        /// </summary>
        public float ZPosition { get; set; }
        public float XRotation { get; set; }
        public float YRotation { get; set; }
        public float ZRotation { get; set; }
        public Curve Curve { get; set; }
        public uint FOV { get; set; }
        public bool Orthographic { get; set; }
    }

    public class Curve {
        public uint AX { get; set; }
        public uint AY { get; set; }
        public uint BX { get; set; }
        public uint BY { get; set; }
    }
}

