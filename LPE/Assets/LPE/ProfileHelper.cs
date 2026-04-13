using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace LPE {
    public static class ProfileHelper {
        private static readonly Dictionary<string, CustomSampler> Samplers = new Dictionary<string, CustomSampler>();

        public static void Start(string name) {
            if (!Samplers.TryGetValue(name, out var sampler)) {
                sampler = CustomSampler.Create(name);
                Samplers[name] = sampler;
            }

            sampler.Begin();
        }

        public static void End(string name) {
            if (Samplers.TryGetValue(name, out var sampler)) {
                sampler.End();
            }
            else {
                Debug.LogWarning($"ProfileHelper: End called for '{name}' without a matching Start.");
            }
        }
        public static ProfilerScope Sample(string name) {
            Start(name);
            return new ProfilerScope(name);
        }
        public struct ProfilerScope : IDisposable {
            private readonly string _name;
            public ProfilerScope(string name) => _name = name;
            public void Dispose() => End(_name);
        }

    }
}