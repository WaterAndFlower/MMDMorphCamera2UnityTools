using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerBearEditor.Math {
    public class float3 {
        public float x = 0, y = 0, z = 0;
        public float3(float m0, float m1, float m2) {
            this.x = m0;
            this.y = m1;
            this.z = m2;
        }

        public float3(float3 i) {
            this.x = i.x;
            this.y = i.y;
            this.z = i.z;
        }

        public static float3 operator *(float3 v, float m) {
            return new(m * v.x, m * v.y, m * v.z);
        }

        public static float3 operator *(float m, float3 v) {
            return new(m * v.x, m * v.y, m * v.z);
        }

        public static float3 operator /(float3 v, float m) {
            return new(v.x / m, v.y / m, v.z / m);
        }

        public static float3 operator +(float3 v, float m) {
            return new(m + v.x, m + v.y, m + v.z);
        }

        public static float3 operator +(float3 v, float3 m) {
            return new(m.x + v.x, m.y + v.y, m.z + v.z);
        }
        public static float3 operator -(float3 v, float3 m) {
            return new(v.x - m.x, v.y - m.y, v.z - m.z);
        }
    }
    public class float3x3 {
        float[] p = new float[9];
        public float3 c0 {
            get { return new(p[0], p[3], p[6]); }
        }
        public float3 c1 { 
            get { return new(p[1], p[4], p[7]); } 
        }
        public float3 c2 { 
            get { return new(p[2], p[5], p[8]); }
        }
        public float3x3(float m0, float m1, float m2, float m3, float m4, float m5, float m6, float m7, float m8) {
            p[0] = m0;
            p[1] = m1;
            p[2] = m2;
            p[3] = m3;
            p[4] = m4;
            p[5] = m5;
            p[6] = m6;
            p[7] = m7;
            p[8] = m8;
        }
    }
    public class MathP {

    }
}
