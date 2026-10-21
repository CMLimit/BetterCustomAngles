using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace BetterCustomAngles
{
    [Serializable]
    public class AngleNoteData
    {
        public float angle;
        public string note;
    }

    [Serializable]
    public class AngleStoreData
    {
        public List<float> history = new List<float>();
        public List<float> favorites = new List<float>();
        public List<AngleNoteData> notes = new List<AngleNoteData>();
    }

    public class AngleStore
    {
        private readonly string filePath;
        private readonly List<float> history = new List<float>();
        private readonly List<float> favorites = new List<float>();
        private readonly Dictionary<long, string> notes = new Dictionary<long, string>();
        private int limit;

        public event Action HistoryChanged;
        public event Action FavoritesChanged;
        public event Action NotesChanged;

        public AngleStore(string modPath, int historyLimit)
        {
            filePath = Path.Combine(modPath, "angles.json");
            limit = Mathf.Clamp(historyLimit, 5, 200);
            Load();
        }

        public IReadOnlyList<float> History => history;
        public IReadOnlyList<float> Favorites => favorites;
        public int Limit => limit;

        public void SetLimit(int value)
        {
            int next = Mathf.Clamp(value, 5, 200);
            if (next == limit)
            {
                return;
            }
            limit = next;
            bool changed = TrimHistory();
            if (changed)
            {
                Save();
                HistoryChanged?.Invoke();
            }
        }

        public void AddHistory(float value)
        {
            value = AngleUtil.Round6(value);
            long key = AngleUtil.Key(value);

            for (int i = history.Count - 1; i >= 0; i--)
            {
                if (AngleUtil.Key(history[i]) == key)
                {
                    history.RemoveAt(i);
                }
            }

            history.Insert(0, value);
            TrimHistory();
            Save();
            HistoryChanged?.Invoke();
        }

        public bool IsFavorite(float value)
        {
            long key = AngleUtil.Key(value);
            for (int i = 0; i < favorites.Count; i++)
            {
                if (AngleUtil.Key(favorites[i]) == key)
                {
                    return true;
                }
            }
            return false;
        }

        public bool ToggleFavorite(float value)
        {
            value = AngleUtil.Round6(value);
            long key = AngleUtil.Key(value);
            for (int i = 0; i < favorites.Count; i++)
            {
                if (AngleUtil.Key(favorites[i]) == key)
                {
                    favorites.RemoveAt(i);
                    Save();
                    FavoritesChanged?.Invoke();
                    return false;
                }
            }

            favorites.Add(value);
            Save();
            FavoritesChanged?.Invoke();
            return true;
        }

        public void RemoveHistory(float value)
        {
            long key = AngleUtil.Key(value);
            for (int i = 0; i < history.Count; i++)
            {
                if (AngleUtil.Key(history[i]) == key)
                {
                    history.RemoveAt(i);
                    Save();
                    HistoryChanged?.Invoke();
                    return;
                }
            }
        }

        public string GetNote(float value)
        {
            return notes.TryGetValue(AngleUtil.Key(value), out string note) ? note : string.Empty;
        }

        public void SetNote(float value, string note)
        {
            long key = AngleUtil.Key(value);
            note = (note ?? string.Empty).Trim();
            if (note.Length == 0)
            {
                if (notes.Remove(key))
                {
                    Save();
                    NotesChanged?.Invoke();
                }
                return;
            }

            if (notes.TryGetValue(key, out string existing) && existing == note)
            {
                return;
            }
            notes[key] = note;
            Save();
            NotesChanged?.Invoke();
        }

        public void ClearHistory()
        {
            if (history.Count == 0)
            {
                return;
            }
            history.Clear();
            Save();
            HistoryChanged?.Invoke();
        }

        private bool TrimHistory()
        {
            bool changed = false;
            while (history.Count > limit)
            {
                history.RemoveAt(history.Count - 1);
                changed = true;
            }
            return changed;
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return;
                }

                var data = JsonConvert.DeserializeObject<AngleStoreData>(File.ReadAllText(filePath));
                if (data == null)
                {
                    return;
                }

                if (data.history != null)
                {
                    var seen = new HashSet<long>();
                    foreach (float raw in data.history)
                    {
                        float v = AngleUtil.Round6(raw);
                        if (seen.Add(AngleUtil.Key(v)))
                        {
                            history.Add(v);
                        }
                    }
                }

                if (data.favorites != null)
                {
                    var seen = new HashSet<long>();
                    foreach (float raw in data.favorites)
                    {
                        float v = AngleUtil.Round6(raw);
                        if (seen.Add(AngleUtil.Key(v)))
                        {
                            favorites.Add(v);
                        }
                    }
                }

                if (data.notes != null)
                {
                    foreach (AngleNoteData entry in data.notes)
                    {
                        if (entry == null || string.IsNullOrEmpty(entry.note))
                        {
                            continue;
                        }
                        notes[AngleUtil.Key(entry.angle)] = entry.note.Trim();
                    }
                }

                TrimHistory();
            }
            catch (Exception e)
            {
                Main.LogException(e);
            }
        }

        public void Save()
        {
            try
            {
                var data = new AngleStoreData
                {
                    history = new List<float>(history),
                    favorites = new List<float>(favorites),
                    notes = new List<AngleNoteData>()
                };
                foreach (KeyValuePair<long, string> pair in notes)
                {
                    data.notes.Add(new AngleNoteData
                    {
                        angle = pair.Key / 1000000f,
                        note = pair.Value
                    });
                }
                File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
            }
            catch (Exception e)
            {
                Main.LogException(e);
            }
        }
    }
}
