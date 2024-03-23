/* PowerBear (Bilibili:大千小熊) Written This Script
 * Do Not Spread This Code To Other Without The Permission.Do NOT Use this code academically/Commercially.请不要在学术上（期刊论文，竞赛或者其他学术活动）商业上使用。
 * 请在规范和符合法律规定的地方使用，本脚本作者不承担一切违法责任。
 * Reference：MMD Encoding/MMD4Mecanim/MMD4Unity Tool/and other's paper/articles
 */

using PowerBearEditor.MMD;
using PowerBearEditor.SharedObject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using PowerBearEditor.Math;
namespace PowerBearEditor {
    public class MmdCameraGenerate : EditorWindow {
        private VmdObject vmdObject = new();
        private string out_put_path = "Assets/CameraClip.anim";
        private string vmd_path { get { return vmdObject.vmd_path; } }
        public UnityEngine.Object vmd_file {
            set { vmdObject.vmd_file = value; }
            get { return vmdObject.vmd_file; }
        }
        private AnimationClip clip;
        GUIContent content, content1;
        public float scale = 1;
        public float frame_rate = 30;
        public bool set_camrae_fov = false;
        public bool set_camrae_perspective = false;
        float3x3 RotationX(float angle) {
            float sine = (float)Mathf.Sin(angle);
            float cose = (float)Mathf.Cos(angle);
            return new(
                1, 0, 0,
                0, cose, -sine,
               0, sine, cose
                );
        }
        float3x3 RotationY(float angle) {
            float sine = (float)Mathf.Sin(angle);
            float cose = (float)Mathf.Cos(angle);
            return new(
                cose, 0, sine,
                0, 1, 0,
               -sine, 0, cose
                );
        }
        float3 mul(float3x3 matrix, float3 v) {
            return new(
               v.x * matrix.c0 + v.y * matrix.c1 + v.z * matrix.c2
                );
        }
        float3 normalize(float3 v) {
            return new(v / (float)(Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z)));
        }
        private void domain() {
            if (vmd_file == null) {
                Debug.LogError("Not Select A VMD file");
                return;
            }

            clip = new AnimationClip();
            clip.frameRate = frame_rate;
            clip.legacy = false;

            var delta = 1 / clip.frameRate;

            Debug.Log("vmd path: " + vmd_path);
            using var st = File.Open(vmd_path, FileMode.Open);
            var data = ConvertVMD.Read(st);
            st.Dispose();

            List<CameraFrame> frames = data.CameraFrames;

            List<Keyframe> xPos = new();
            List<Keyframe> yPos = new();
            List<Keyframe> zPos = new();
            List<Keyframe> xRot = new();
            List<Keyframe> yRot = new();
            List<Keyframe> zRot = new();
            List<Keyframe> wRot = new();
            List<Keyframe> fov = new();
            List<Keyframe> cameraType = new();

            foreach (var item in frames) {
                var _f = item.FrameIndex * delta;
                // Right Hand Axies Up Y
                float3 current_camera_anchor_mmd = new(item.XPosition, item.YPosition, item.ZPosition);
                current_camera_anchor_mmd *= scale;
                float distance = item.Distance * scale;
                // pos far
                float3 camera_pos_point_mmd = new(0, 0, 1);
                var rtx = RotationX(item.XRotation);
                var rty = RotationY(item.YRotation);
                float3 rt_pos_mmd = mul(rtx, camera_pos_point_mmd);
                rt_pos_mmd = mul(rty, rt_pos_mmd);
                rt_pos_mmd *= distance * -1;
                rt_pos_mmd += current_camera_anchor_mmd;

                // Unity Axies
                float3 anchor_unity = new(-1 * current_camera_anchor_mmd.x, current_camera_anchor_mmd.y, current_camera_anchor_mmd.z);
                anchor_unity *= scale;
                float3 rt_pos_unity = new(-1 * rt_pos_mmd.x, rt_pos_mmd.y, rt_pos_mmd.z);
                rt_pos_unity *= scale;
                float3 view_dir = normalize(rt_pos_unity - anchor_unity);
                float3 final_camera_pos = rt_pos_unity;

                xPos.Add(new(_f, final_camera_pos.x));
                yPos.Add(new(_f, final_camera_pos.y));
                zPos.Add(new(_f, final_camera_pos.z));

                var quaternion = Quaternion.LookRotation(new(-view_dir.x, -view_dir.y, -view_dir.z));
                var z_auaternion = Quaternion.Euler(0, 0, item.ZRotation * Mathf.Rad2Deg);
                // first L and R
                quaternion = quaternion * z_auaternion;
                //var quaternion = Quaternion.Euler(new Vector3(item.XRotation * Mathf.Rad2Deg, item.YRotation * Mathf.Rad2Deg, item.ZRotation * Mathf.Rad2Deg));
                Vector3 rotvec = new(item.XRotation, item.YRotation, item.ZRotation);
                rotvec *= Mathf.Rad2Deg;
                quaternion = Quaternion.Euler(new(-rotvec.x, 180 - rotvec.y, -rotvec.z));
                xRot.Add(new(_f, quaternion.x));
                yRot.Add(new(_f, quaternion.y));
                zRot.Add(new(_f, quaternion.z));
                wRot.Add(new(_f, quaternion.w));

                if (set_camrae_fov)
                    fov.Add(new(_f, item.FOV));

                if (set_camrae_perspective)
                    cameraType.Add(new(_f, item.Orthographic == true ? 1 : 0));

            }

            var xPostionCurve = new AnimationCurve(xPos.ToArray());
            var yPostionCurve = new AnimationCurve(yPos.ToArray());
            var zPostionCurve = new AnimationCurve(zPos.ToArray());
            var xRotationCurve = new AnimationCurve(xRot.ToArray());
            var yRotationCurve = new AnimationCurve(yRot.ToArray());
            var zRotationCurve = new AnimationCurve(zRot.ToArray());
            var wRotationCurve = new AnimationCurve(wRot.ToArray());
            var fovCurve = new AnimationCurve(fov.ToArray());
            var typeCure = new AnimationCurve(cameraType.ToArray());

            clip.SetCurve("", typeof(Transform), "localPosition.x", xPostionCurve);
            clip.SetCurve("", typeof(Transform), "localPosition.y", yPostionCurve);
            clip.SetCurve("", typeof(Transform), "localPosition.z", zPostionCurve);
            clip.SetCurve("", typeof(Transform), "localRotation.x", xRotationCurve);
            clip.SetCurve("", typeof(Transform), "localRotation.y", yRotationCurve);
            clip.SetCurve("", typeof(Transform), "localRotation.z", zRotationCurve);
            clip.SetCurve("", typeof(Transform), "localRotation.w", wRotationCurve);
            if (set_camrae_fov)
                clip.SetCurve("", typeof(Camera), "field of view", fovCurve);
            if (set_camrae_perspective)
                clip.SetCurve("", typeof(Camera), "orthographic", typeCure);
            out_put_path = Path.Combine(Path.GetDirectoryName(vmd_path), "camera.anim");
            AssetDatabase.CreateAsset(clip, out_put_path);
        }
        public static MmdCameraGenerate Instance { get; private set; }
        [MenuItem("Tools/PowerBear/MMD Camera Animation Clip Generate Tool")]
        public static void ShowWindow() {
            Instance = GetWindow<MmdCameraGenerate>();
            Instance.Show();
        }
        void Test() {
            AnimationCurve curve;
            AnimationClip clip = new AnimationClip();
            clip.legacy = false;

            List<Keyframe> keys_x = new();
            using var st = File.Open(vmd_path, FileMode.Open);
            var vmd_data = ConvertVMD.Read(st);
            st.Dispose();

            List<CameraFrame> frames = vmd_data.CameraFrames.OrderBy(x => x.FrameIndex).ToList();
            float delta = 1 / 30.0f;
            for (int i = 0; i < frames.Count; i++) {
                if (i % 2 == 0) {
                    keys_x.Add(new Keyframe(i, 1.0f));
                } else {
                    keys_x.Add(new Keyframe(i, 1f));
                }
            }
            keys_x.Add(new(0, 0));
            curve = new AnimationCurve(keys_x.ToArray());
            clip.SetCurve("", typeof(Transform), "localPosition.x", curve);

            out_put_path = Path.Combine(Path.GetDirectoryName(vmd_path), "test.anim");
            AssetDatabase.CreateAsset(clip, out_put_path);
        }
        private void OnGUI() {
            GUILayout.Label("Camera Generate AnimationClip");
            GUILayout.Label("Made By PowerBear | 生成动画资源文件工具");
            GUILayout.Space(20);

            GUILayout.Label("Camera VMD");
            vmd_file = EditorGUILayout.ObjectField(vmd_file, typeof(UnityEngine.Object), false) as UnityEngine.Object;
            GUILayout.Label($"Frame Rate {frame_rate} fps");
            frame_rate = GUILayout.HorizontalSlider(frame_rate, 30, 60);
            GUILayout.Space(10);
            GUILayout.Label($"Scale {scale}X");
            scale = GUILayout.HorizontalSlider(scale, 0.001f, 10f);
            GUILayout.Space(10);
            GUILayout.Label($"Other Settings");
            content = content == null ? new GUIContent("Set camera FOV") : content;
            content1 = content1 == null ? new GUIContent("Set camera type (orthogonal/perspective)") : content1;
            set_camrae_fov = GUILayout.Toggle(set_camrae_fov, content);
            set_camrae_perspective = GUILayout.Toggle(set_camrae_perspective, content1);
            GUILayout.Space(20);
            if (GUILayout.Button("Generate Camera Animation Clip")) {
                domain();
            }
            //if (GUILayout.Button("Generate Test Animation Clip")) {
            //    Test();
            //}
            GUILayout.TextArea("实验性功能，数据生成不一定完美！在Timeline或者其他地方，注意取消掉Remove Offest功能。");
        }
    }
}
