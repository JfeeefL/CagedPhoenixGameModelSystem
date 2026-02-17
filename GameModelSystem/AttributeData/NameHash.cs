using System;
using UnityEngine;
using Sirenix.OdinInspector; // 引用 Odin

namespace GameModelSystem
{
    [Serializable, InlineProperty] // InlineProperty 让它在 List 里显示更紧凑
    public struct NameHash : IEquatable<NameHash>, IComparable<NameHash>
    {
        [SerializeField, HideInInspector] 
        private string _name;

        [SerializeField, HideInInspector] 
        private int _hash;

        public int Hash
        {
            get
            {
                if (_hash == 0 && !string.IsNullOrEmpty(_name))
                {
                    _hash = _name.GetHashCode();
                }
                return _hash;
            }
        }

        public string Name => _name;

        public NameHash(string name)
        {
            _name = name;
            _hash = string.IsNullOrEmpty(name) ? 0 : name.GetHashCode();
        }

        public static NameHash FromName(string name) => new NameHash(name);

        public static implicit operator NameHash(string name) => new NameHash(name);
        public static implicit operator int(NameHash hash) => hash.Hash;
        public static implicit operator string(NameHash hash) => hash._name;

        public bool Equals(NameHash other) => this.Hash == other.Hash;
        public override bool Equals(object obj) => obj is NameHash other && Equals(other);
        public override int GetHashCode() => Hash;
        public override string ToString() => string.IsNullOrEmpty(_name) ? "Empty" : _name;
        
        public int CompareTo(NameHash other) => Hash.CompareTo(other.Hash);

        public static bool operator ==(NameHash left, NameHash right) => left.Equals(right);
        public static bool operator !=(NameHash left, NameHash right) => !left.Equals(right);
    }
}