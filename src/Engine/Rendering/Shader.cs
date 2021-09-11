
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#nullable disable warnings

namespace ZargoEngine.Rendering
{
    public class Shader : IDisposable
    {
        // these are locations of the uniforms in shader
        private readonly int ambientStrength, ambientColor,
                             sunIntensity   , sunAngle,
                             sunColor       , viewpos ;

        private const string IndirectLight = "IndirectLight",
                             ShadowSupport = "HasShadows";

        private readonly int[] PointLightLocations = new int[10];
        private readonly int LightSpaceLoc, BiasLoc;
        private readonly int TimeLoc;

        public readonly int ModelMatrixLoc, viewProjectionLoc;

        public int program;

        public bool hasIndirectLight, SupportsShadows, hasLighting;

        public string vertexPath;
        public string fragmentPath;

        public Shader(in string vertexPath, in string fragmentPath)
        {
            this.vertexPath = vertexPath;
            this.fragmentPath = fragmentPath;
            string vertexSource = string.Empty, fragmentSource = string.Empty;

            using (StreamReader reader = new StreamReader(vertexPath))
            {
                vertexSource = reader.ReadToEnd();
            }

            using (StreamReader reader = new StreamReader(fragmentPath))
            {

                // this if because of ignoring non3d shader reading first line maybe corrupt shader if first line of the shader is not empty that can cause crash
                if (Path.GetExtension(fragmentPath) == EngineConstsants.frag)
                {
                    string firstLine = reader.ReadLine();
                    hasIndirectLight = firstLine.Contains(IndirectLight);
                    SupportsShadows = firstLine.Contains(ShadowSupport);
                    Debug.LogIf(SupportsShadows, $"{Path.GetFileName(fragmentPath)} supports shadows");
                }

                fragmentSource = reader.ReadToEnd();
            }

            CompileShader(vertexSource, fragmentSource);

            ModelMatrixLoc = GL.GetUniformLocation(program, "model");
            viewProjectionLoc = GL.GetUniformLocation(program, "viewProjection");
            TimeLoc = GL.GetUniformLocation(program, "time");

            // basic lighting uniforms
            ambientStrength = GL.GetUniformLocation(program, nameof(ambientStrength));
            ambientColor    = GL.GetUniformLocation(program, nameof(ambientColor));
            sunIntensity    = GL.GetUniformLocation(program, nameof(sunIntensity)); 
            sunAngle        = GL.GetUniformLocation(program, nameof(sunAngle)); 
            sunColor        = GL.GetUniformLocation(program, nameof(sunColor));
            viewpos         = GL.GetUniformLocation(program, nameof(viewpos));

            if (hasIndirectLight)
            {
                for (int i = 0; i < PointLightLocations.Length; i++)
                {
                    PointLightLocations[i] = GL.GetUniformLocation(program, $"lights[i]");
                }
            }

            if (SupportsShadows)
            {
                LightSpaceLoc = GL.GetUniformLocation(program, "lightSpaceMatrix");
                BiasLoc = GL.GetUniformLocation(program, "bias");
            }
        }

        public void SetDefaults(RenderHandeller handeller)
        {
            SetFloat(TimeLoc, Time.time);
            // set handeller uniforms
            SetFloat(ambientStrength, handeller.ambientStrength);
            SetFloat(sunAngle, handeller.sun.angle);
            SetFloat(sunIntensity, handeller.sun.intensity);
            SetVector3(viewpos, handeller.camera.Position);
            SetVector3Sys(ambientColor, handeller.ambientColor);
            SetVector4Sys(sunColor, handeller.sun.sunColor);
            // set matrices
            SetMatrix4(viewProjectionLoc, handeller.camera.ViewMatrix * handeller.camera.projectionMatrix);

            if (SupportsShadows)
            {
                SetMatrix4(LightSpaceLoc, Renderer3D.lightSpaceMatrix, true);
                SetFloat(BiasLoc, Renderer3D.Bias);
            }

            if (hasIndirectLight)
            {
                // loop lights (point, spot)
                for (short i = 0; i < RenderHandeller.instance.lights.Count; i++)
                {
                    Light light = RenderHandeller.instance.lights[i];
                    SetFloat($"pointLights[{i}].intensity", light.intensity);
                    SetInt($"pointLights[{i}].type", (byte)light.lightMode);
                    SetVector3($"pointLights[{i}].position", light.transform.position);
                    SetVector3($"pointLights[{i}].direction", -light.transform.up);
                    SetVector3Sys($"pointLights[{i}].color", light.color);
                }
                SetInt("lightCount", RenderHandeller.instance.lights.Count);
            }
        }

        public void Recompile()
        {
            string vertexSource = string.Empty, fragmentSource = string.Empty;

            using (StreamReader reader = new StreamReader(vertexPath))
            {
                vertexSource = reader.ReadToEnd();
            }

            using (StreamReader reader = new StreamReader(fragmentPath)) 
            {
                hasIndirectLight = reader.ReadLine().Contains(IndirectLight);
                fragmentSource = reader.ReadToEnd();
            }

            CompileShader(vertexSource, fragmentSource);
        }

        public void CompileShader(in string vertexSource, in string fragmentSource)
        {
            int vertexID = GL.CreateShader(ShaderType.VertexShader);

            GL.ShaderSource(vertexID, vertexSource);
            GL.CompileShader(vertexID);
            Console.WriteLine(GL.GetError());

            GL.GetShader(vertexID, ShaderParameter.CompileStatus, out int log);

            if (log == 0)
            {
                GL.GetShaderInfoLog(vertexID, out string infoLog);
                Console.WriteLine(infoLog);
                throw new Exception("vertex shader failed");
            }

            int fragmentID = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(fragmentID, fragmentSource);
            GL.CompileShader(fragmentID);

            Console.WriteLine(GL.GetError());

            GL.GetShader(fragmentID, ShaderParameter.CompileStatus, out log);

            if (log == 0)
            {
                GL.GetShaderInfoLog(fragmentID, out string infoLog);
                Console.WriteLine(infoLog);

                Console.WriteLine(fragmentPath);
                throw new Exception("fragment shader failed");
            }

            program = GL.CreateProgram();
            GL.AttachShader(program, vertexID);
            GL.AttachShader(program, fragmentID);

            GL.LinkProgram(program);

            Console.WriteLine(GL.GetError());

            GL.GetShader(program, ShaderParameter.CompileStatus, out log);

            if (log == 0)
            {
                GL.GetShaderInfoLog(program, out string infoLog);
                Console.WriteLine(infoLog);
                throw new Exception("linking shaders failed");
            }

            Console.WriteLine("Shader compiled sucsesfully");

            GL.DetachShader(program, vertexID);
            GL.DetachShader(program, fragmentID);
            GL.DeleteShader(vertexID);
            GL.DeleteShader(fragmentID);
        }

        public void Use() => GL.UseProgram(program);

        public void Detach() => GL.UseProgram(0);
        public static void DetachShader() => GL.UseProgram(0);

        public int GetAttribLocation(string name) => GL.GetAttribLocation(program, name);

        // SET
        // Uniforms
        public int GetUniformLocation(in string name) => GL.GetUniformLocation(program, name);
        public static int GetUniformLocation(in int program, in string name) => GL.GetUniformLocation(program, name);

        public void SetIntLocation(in int location, in int value) => GL.Uniform1(location, value);
        public void SetInt(in string name, in int value) => GL.Uniform1(GL.GetUniformLocation(program, name), value);
        public void SetFloat(in string name, in float value) => GL.Uniform1(GL.GetUniformLocation(program, name), value);
        public void SetVector2i(in string name, in Vector2i value) => GL.Uniform2(GL.GetUniformLocation(program, name), value);
        public void SetVector2(in string name, in Vector2 value) => GL.Uniform2(GL.GetUniformLocation(program, name), value);
        public void SetVector3(in string name, in Vector3 value) => GL.Uniform3(GL.GetUniformLocation(program, name), value);
        public void SetVector4(in string name, in Vector4 value) => GL.Uniform4(GL.GetUniformLocation(program, name), value);
        public void SetMatrix4(in string name, Matrix4 value, bool transpose = true) => GL.UniformMatrix4(GL.GetUniformLocation(program, name), transpose, ref value);
        public void SetMatrix4(in string name, ref float value, int count, bool transpose = true) => GL.UniformMatrix4(GL.GetUniformLocation(program, name), count, transpose, ref value);
        public void SetMatrix4(in string name, ref Matrix4 value, bool transpose = true) => GL.UniformMatrix4(GL.GetUniformLocation(program, name), transpose, ref value);
        public void SetMatrix4Location(in int location, Matrix4 value, bool transpose = true) => GL.UniformMatrix4(location, transpose, ref value);
        public void SetMatrix4Location(in int location, ref Matrix4 value, bool transpose = true) => GL.UniformMatrix4(location, transpose, ref value);

        // openTK
        public static void SetInt(in int location, in int value) => GL.Uniform1(location, value);
        public static void SetFloat(in int location, in float value) => GL.Uniform1(location, value);
        public static void SetVector2(in int location, in Vector2 value) => GL.Uniform2(location, value);
        public static void SetVector3(in int location, in Vector3 value) => GL.Uniform3(location, value);
        public static void SetVector4(in int location, in Vector4 value) => GL.Uniform4(location, value);

        // system
        public void SetVector2Sys(in string name, in System.Numerics.Vector2 value) => GL.Uniform2(GL.GetUniformLocation(program, name), value.X, value.Y);
        public void SetVector3Sys(in string name, in System.Numerics.Vector3 value) => GL.Uniform3(GL.GetUniformLocation(program, name), value.X, value.Y, value.Z);
        public void SetVector4Sys(in string name, in System.Numerics.Vector4 value) => GL.Uniform4(GL.GetUniformLocation(program, name), value.X, value.Y, value.Z, value.W);

        public static void SetVector2Sys(in int location, in System.Numerics.Vector2 value) => GL.Uniform2(location, value.X, value.Y);
        public static void SetVector3Sys(in int location, in System.Numerics.Vector3 value) => GL.Uniform3(location, value.X, value.Y, value.Z);
        public static void SetVector4Sys(in int location, in System.Numerics.Vector4 value) => GL.Uniform4(location, value.X, value.Y, value.Z, value.W);

        public static void SetMatrix4(in int location, Matrix4 value, bool transpose = true) => GL.UniformMatrix4(location, transpose, ref value);
        public static void SetMatrix4(in int location, ref Matrix4 value, bool transpose = true) => GL.UniformMatrix4(location, transpose, ref value);

        // system
        // for vec2 vec3 vec4
        public unsafe void GetFloat<T>(in string name, out T value) where T : unmanaged
        {
            float* ptr = (float*)Marshal.AllocHGlobal(Marshal.SizeOf<T>());
            GL.GetUniform(program, GL.GetUniformLocation(program, name), ptr);
            value = Marshal.PtrToStructure<T>((IntPtr)ptr);
        }

        public unsafe void GetFloatRef<T>(in string name, ref T value) where T : unmanaged
        {
            GL.GetUniform(program, GL.GetUniformLocation(program, name), (float*)Unsafe.AsPointer(ref value));
        }

        // for vec2i vec3i etc
        public unsafe void GetInt<T>(in string name, out T value) where T : unmanaged
        {
            int* ptr = (int*)Marshal.AllocHGlobal(Marshal.SizeOf<T>());
            GL.GetUniform(program, GL.GetUniformLocation(program, name), ptr);
            value = Marshal.PtrToStructure<T>((IntPtr)ptr);
        }

        public void Dispose()
        {
            GL.DeleteShader(program);
            GC.SuppressFinalize(this);
        }
    }
}
