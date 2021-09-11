using System.Collections.Generic;
using OpenTK.Mathematics;
using Assimp;

#nullable disable warnings
// https://github.com/mellinoe/veldrid-samples/blob/master/src/AnimatedMesh/Application/AnimatedMesh.cs
namespace ZargoEngine.Rendering
{
    using Helper;
    using Mathmatics;
    using System;
    using aiMatrix4x4  = Matrix4x4;
    using aiQuaternion = Assimp.Quaternion;
    using aiScene = Assimp.Scene;

    public class Animator
    { 
        public float animationTimeScale = 1f;
        private readonly Animation animation;
        private readonly Dictionary<string, int> boneIDsByName = new Dictionary<string, int>();
        private readonly Assimp.Mesh firstMesh;
        private readonly aiMatrix4x4 rootNodeInverseTransform;
        public readonly aiMatrix4x4[] boneMatrices;
        private readonly Line[] lines;
        private readonly List<Node> nodes ;
        private readonly Node rootNode;
        private double previousAnimSeconds = 0;

        public Animator(aiScene scene, Dictionary<string, int> _boneIDsByName)
        {
            this.boneIDsByName = _boneIDsByName;
            animation = scene.Animations[0];
            firstMesh = scene.Meshes[0];

            boneMatrices = new aiMatrix4x4[firstMesh.Bones.Count];

            lines = new Line[firstMesh.Bones.Count + 4]; 

            ushort i = 0;
            for (; i < lines.Length; i++) // initializes all of the linse
            {
                lines[i] = new Line(Vector3.Zero, Vector3.Zero);
            }

            // logs all of the bones
            for (i = 0; i < firstMesh.Bones.Count; i++)
            {
                Bone bone = firstMesh.Bones[i];
                boneMatrices[i] = aiMatrix4x4.Identity;
                Console.WriteLine($"{i} {bone.Name}");
            }

            nodes = new List<Node>(firstMesh.Bones.Count);
            Debug.Log("node names");
            rootNode = FindRootNode(scene);
            int recIndex = 0;

            recurisiveLog(rootNode);

            // logs root bone node and all of its childs
            void recurisiveLog(Node node)
            {
                Console.WriteLine($"{recIndex} {node.Name}");
                nodes.Add(node);
                recIndex++;
                for (int i = 0; i < node.Children.Count; i++)
                {
                    recurisiveLog(node.Children[i]);
                }
            }

            Debug.Log("Bone Count: " + firstMesh.Bones.Count);

            rootNodeInverseTransform = rootNode.Transform;
            rootNodeInverseTransform.Inverse();
        }

        private Node FindRootNode(aiScene scene)
        {
            Node findedNode = default;

            find(scene.RootNode);

            void find(Node node)
            {
                if (findedNode != default) return;

                foreach (var child in node.Children)
                {
                    if (findedNode != default) break;

                    // node parent is not bone
                    if (firstMesh.Bones.FindIndex(b => b.Name == child.Parent.Name) == -1)
                    {
                        // the node is bone 
                        if (firstMesh.Bones.FindIndex(b => b.Name == child.Name) != -1)
                        {
                            findedNode = child;
                            break;
                        }
                    }
                    find(child);
                }
            }

            // finded
            if (Debug.Assert(findedNode == default, "root bone is not founded") == false)
            {
                Debug.Log($"root bone: {findedNode.Name}");
            }

            return findedNode;
        }

        public void AsignGameObject(GameObject go)
        {
            go.OnUpdate += UpdateAnimation;
        }

        private void UpdateAnimation()
        {
            double totalSeconds = animation.DurationInTicks * animation.TicksPerSecond;
            double newSeconds = previousAnimSeconds + (Time.DeltaTime * animationTimeScale);
            newSeconds %= totalSeconds;
            previousAnimSeconds = newSeconds;

            double ticks = newSeconds * animation.TicksPerSecond;

            recIndex = -1;
            UpdateChannel(ticks, rootNode, aiMatrix4x4.Identity);
            // SecondMethod(ticks);
        }

        private int recIndex;
        
        private void UpdateChannel(in double time, Node node, in aiMatrix4x4 parentTransform)
        {
            aiMatrix4x4 nodeTransformation = node.Transform;
        
            if (GetChannel(node, out NodeAnimationChannel channel))
            {
                aiMatrix4x4 scale = InterpolateScale(time, channel);
                aiMatrix4x4 rotation = InterpolateRotation(time, channel);
                aiMatrix4x4 translation = InterpolateTranslation(time, channel);
        
                nodeTransformation = scale * rotation * translation;
            }
        
            aiMatrix4x4 globalTransform = parentTransform * nodeTransformation;

            if (boneIDsByName.TryGetValue(node.Name, out int boneID))
            {
                aiMatrix4x4 m = globalTransform * firstMesh.Bones[boneID].OffsetMatrix;
                boneMatrices[boneID] = m;
            }
            
            recIndex++; // recurisive index
            
            foreach (Node childNode in node.Children)
            {
                // this line creates debug lines
                lines[recIndex].Invalidate(nodeTransformation.ExportTranslation(), parentTransform.ExportTranslation());
                UpdateChannel(time, childNode, globalTransform);
            }
        }

        private void SecondMethod(in double time)
        {
            for (ushort i = 0; i < firstMesh.Bones.Count; i++)
            {
                Bone bone = firstMesh.Bones[i];
                Node node = nodes.Find(n => n.Name == bone.Name);

                boneMatrices[i] = bone.OffsetMatrix;

                if (GetChannel(node, out NodeAnimationChannel channel))
                {
                    aiMatrix4x4 scale = InterpolateScale(time, channel);
                    aiMatrix4x4 rotation = InterpolateRotation(time, channel);
                    aiMatrix4x4 translation = InterpolateTranslation(time, channel);
                
                    boneMatrices[i] = scale * rotation * translation * boneMatrices[i];
                }
                
                Node parent = node;

                while (parent != null) 
                {
                    boneMatrices[i] = parent.Transform * boneMatrices[i];
                    parent = parent.Parent;
                }
            }
        }

        private static aiMatrix4x4 InterpolateTranslation(in double time, NodeAnimationChannel channel)
        {
            Vector3D position;

            if (channel.PositionKeyCount == 1)
            {
                position = channel.PositionKeys[0].Value;
            }
            else
            {
                int frameIndex = 0;
                for (ushort i = 0; i < channel.PositionKeyCount - 1; i++)
                {
                    if (time < (float)channel.PositionKeys[i + 1].Time)
                    {
                        frameIndex = i;
                        break;
                    }
                }

                VectorKey currentFrame = channel.PositionKeys[frameIndex];
                VectorKey nextFrame = channel.PositionKeys[(frameIndex + 1) % channel.PositionKeyCount];

                double delta = (time - (float)currentFrame.Time) / (float)(nextFrame.Time - currentFrame.Time);

                Vector3D start = currentFrame.Value;
                Vector3D end = nextFrame.Value;
                position = (start + (float)delta * (end - start));
            }

            return aiMatrix4x4.FromTranslation(position);
        }

        private static aiMatrix4x4 InterpolateRotation(in double time, NodeAnimationChannel channel)
        {
            aiQuaternion rotation;

            if (channel.RotationKeyCount == 1)
            {
                rotation = channel.RotationKeys[0].Value;
            }
            else
            {
                int frameIndex = 0;
                for (ushort i = 0; i < channel.RotationKeyCount - 1; i++)
                {
                    if (time < (float)channel.RotationKeys[i + 1].Time)
                    {
                        frameIndex = i;
                        break;
                    }
                }

                QuaternionKey currentFrame = channel.RotationKeys[frameIndex];
                QuaternionKey nextFrame = channel.RotationKeys[(frameIndex + 1) % channel.RotationKeyCount];

                double delta = (time - (float)currentFrame.Time) / (float)(nextFrame.Time - currentFrame.Time);

                aiQuaternion start = currentFrame.Value;
                aiQuaternion end = nextFrame.Value;
                rotation = aiQuaternion.Slerp(start, end, (float)delta);
                rotation.Normalize();
            }

            return rotation.GetMatrix();
        }

        private static aiMatrix4x4 InterpolateScale(in double time, NodeAnimationChannel channel)
        {
            Vector3D scale;

            if (channel.ScalingKeyCount == 1)
            {
                scale = channel.ScalingKeys[0].Value;
            }
            else
            {
                int frameIndex = 0;
                for (ushort i = 0; i < channel.ScalingKeyCount - 1; i++)
                {
                    if (time < (float)channel.ScalingKeys[i + 1].Time)
                    {
                        frameIndex = i;
                        break;
                    }
                }

                VectorKey currentFrame = channel.ScalingKeys[frameIndex];
                VectorKey nextFrame = channel.ScalingKeys[(frameIndex + 1) % channel.ScalingKeyCount];

                double delta = (time - (float)currentFrame.Time) / (float)(nextFrame.Time - currentFrame.Time);

                Vector3D start = currentFrame.Value;
                Vector3D end = nextFrame.Value;

                scale = (start + (float)delta * (end - start));
            }

            return aiMatrix4x4.FromScaling(scale);
        }

        private bool GetChannel(Node node, out NodeAnimationChannel channel)
        {
            foreach (NodeAnimationChannel c in animation.NodeAnimationChannels)
            {
                if (c.NodeName == node.Name)
                {
                    channel = c;
                    return true;
                }
            }

            channel = null;
            return false;
        }
    }
}