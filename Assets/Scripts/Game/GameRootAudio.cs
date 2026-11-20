using UnityEngine;
using Thatchwick.Core.Audio;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Family;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;
using Thatchwick.Game.Audio;

namespace Thatchwick.Game
{
    /// <summary>
    /// Ses bağlantıları (belge 43 ve 45): oyun olayları → ses anahtarı. Hangi olayın hangi sesi çaldığı Core'daki
    /// <see cref="SoundPicker"/>'da; burada yalnızca olaylara abone olunur ve çalınır. Ses dosyası henüz yoksa
    /// (soundbank'te anahtar yok) çağrılar sessizce geçer.
    /// </summary>
    public sealed partial class GameRoot
    {
        private bool _audioMenuOpen, _audioStorageOpen, _audioReady;
        private int _audioAkce = int.MinValue;
        private float _animalCallAt;
        private long _roosterDay = -1;
        private const float AnimalCallMin = 7f, AnimalCallMax = 18f, AnimalCallRange = 14f;

        private static void Sfx(string key, Vector3? pos = null, long entity = 0, float volume = 1f)
        {
            if (key == null) return;
            var a = AudioService.Instance;
            if (a != null) a.Play(key, pos, entity, volume);
        }

        /// <summary>Sistemler kurulduktan sonra bir kez: olay abonelikleri.</summary>
        private void AttachAudio()
        {
            _audioReady = true;
            _audioAkce = int.MinValue;
            ApplySavedVolumes();
            if (_buildService != null) _buildService.Furnished += _ => { _account?.Count("act:furnish"); Sfx(SoundKeys.UiPlace); };
            _life.Log.Added += e => { if (!e.Transient) Sfx(SoundPicker.Notice((int)e.Level)); };
            _life.Ledger.DayEnded += _ => Sfx(SoundKeys.StingDayEnd);
            _life.Injuries.Injured += (c, injury) => Exclaim(c, "pain");
            _life.Decor.Collapsed += _ => Sfx(SoundKeys.ShelfCollapse);
            if (_mines != null) _mines.CaveIn += (m, tiles) => Sfx(SoundKeys.CaveIn);
            if (_buildMode != null) _buildMode.Activity += a => Sfx(SoundPicker.Build(a));
            if (_effects != null) _effects.Applied += (c, i, summary) => { if (i.RemovesItem && i.Id == "chop") Sfx(SoundKeys.TreeFall, VoicePos(c)); };
            if (_life.Wolves != null)
            {
                _life.Wolves.Warning += _ => Sfx(SoundKeys.AnimalWolf, null, 0, 0.5f);          // uzaktan uluma: baskın yaklaşıyor
                _life.Wolves.Raided += r => { if (r.Pack > 0) Sfx(SoundKeys.AnimalWolf); };
            }
            if (_input != null) { _input.Tapped -= OnTapSound; _input.Tapped += OnTapSound; }
        }

        private void OnTapSound(Vector2 _) => Sfx(SoundKeys.UiTap);

        /// <summary>Her karede: panel açılıp kapanması, akçe, hayvan sesleri, şafakta horoz.</summary>
        private void UpdateAudio()
        {
            if (!_audioReady || _household == null || _clock == null) return;
            bool menu = MenuOpen();
            if (menu != _audioMenuOpen) { _audioMenuOpen = menu; Sfx(menu ? SoundKeys.UiOpen : SoundKeys.UiClose); }
            bool storage = _storagePanel != null && _storagePanel.Active;
            if (storage != _audioStorageOpen) { _audioStorageOpen = storage; if (storage) Sfx(SoundKeys.ChestOpen); }

            int akce = _household.Akce;
            if (_audioAkce != int.MinValue && akce != _audioAkce) Sfx(akce > _audioAkce ? SoundKeys.UiCoin : SoundKeys.UiSpend);
            _audioAkce = akce;

            if (_animals == null || _clock.Paused) return;
            // Şafak: hanede yetişkin horoz varsa günde bir kez öter.
            if (_clock.DayIndex != _roosterDay && _clock.DayFraction >= 0.24 && _clock.DayFraction < 0.30)
            {
                _roosterDay = _clock.DayIndex;
                foreach (var a in _animals.Animals)
                    if (a.Species.Id == "animal.chicken" && a.IsMature && !a.IsFemale) { Sfx(SoundKeys.AnimalRooster, Iso.GridToWorld(a.Position.X, a.Position.Y, a.Position.Z), a.Id); break; }
            }
            // Arada bir, komut alan karakterin yakınındaki bir hayvan ses verir (gece seyrek).
            if (Time.unscaledTime < _animalCallAt) return;
            bool night = _clock.DayFraction < 0.22 || _clock.DayFraction > 0.85;
            _animalCallAt = Time.unscaledTime + Mathf.Lerp(AnimalCallMin, AnimalCallMax, Random01()) * (night ? 2.5f : 1f);
            var listener = Commander?.Character;
            if (listener == null || _animals.Animals.Count == 0) return;
            var pick = _animals.Animals[Mathf.Min(_animals.Animals.Count - 1, (int)(Random01() * _animals.Animals.Count))];
            if (pick.Position.Z != listener.Position.Z) return;
            int dx = pick.Position.X - listener.Position.X, dy = pick.Position.Y - listener.Position.Y;
            if (dx * dx + dy * dy > AnimalCallRange * AnimalCallRange) return;
            Sfx(SoundPicker.Animal(pick.Species.Id, pick.IsMature && !pick.IsFemale), Iso.GridToWorld(pick.Position.X, pick.Position.Y, pick.Position.Z), pick.Id, 0.8f);
        }

        /// <summary>Adım: zemine göre ses; kapı eşiğinden geçerken kapı sesi. Uzak bölgedekiler duyulmaz.</summary>
        private void PlayStep(Actor actor, Vector3 pos)
        {
            var c = actor.Character;
            var p = c.Position;
            var here = Commander?.Character;
            if (here != null && (Regions.At(here.Position) != Regions.At(p) || here.Position.Z != p.Z)) { actor.LastTile = p; return; }
            var map = _world.Map;
            if (actor.HasLastTile && !actor.LastTile.Equals(p) && actor.LastTile.Z == p.Z
                && Mathf.Abs(actor.LastTile.X - p.X) + Mathf.Abs(actor.LastTile.Y - p.Y) == 1
                && map.GetEdge(GridMap.EdgeBetween(actor.LastTile, p)) == EdgeKind.Door)
                Sfx(SoundKeys.DoorOpen, pos, c.Id);
            actor.LastTile = p; actor.HasLastTile = true;
            if (c.Stage == AgeStage.Baby) return;
            int weather = _life != null ? (int)_life.Weather.Today(_clock.TotalDays) : 0;
            bool indoor = map.Rooms.RoomAt(p) != null;
            string key = SoundPicker.Step(p.Z < 0, map.GetFloor(p), map.GetRoad(p), Regions.At(p)?.Id, indoor ? 0 : weather,
                !indoor && _clock.Season == Thatchwick.Core.Time.Season.Winter);
            Sfx(key, pos, c.Id, c.IsVillager ? 0.55f : 0.9f);
        }

        /// <summary>İş vuruşu (balta, çapa, kazma, tezgâh...).</summary>
        private void PlayWorkBeat(Actor actor, string icon, Vector3 pos)
        {
            var job = actor.WorkJob as Thatchwick.Core.Jobs.Job;
            Sfx(SoundPicker.Work(icon, job?.InteractionId), pos, actor.Character.Id);
        }

        // ---- karakter sesi (belge 14 bölüm 11): yaratma ekranında seçilen tını ve perde oyunda duyulur ----

        private static float AgeSemitones(AgeStage stage)
        {
            switch (stage)
            {
                case AgeStage.Baby: return 7f;
                case AgeStage.Child: return 5f;
                case AgeStage.Teen: return 2f;
                case AgeStage.Elder: return -1.5f;
                default: return 0f;
            }
        }

        private Vector3? VoicePos(Character c) =>
            c != null ? Iso.GridToWorld(c.Position.X, c.Position.Y, c.Position.Z) : (Vector3?)null;

        /// <summary>Köy diliyle konuşma (hece bankasından; her seferinde farklı).</summary>
        private void Speak(Character c, int textLength, VoiceEmotion emotion = VoiceEmotion.Neutral)
        {
            if (c == null || c.IsDead || c.Stage == AgeStage.Baby || c.Appearance == null) return;
            string voice = VoicePlayer.VoiceKeyFor((int)c.Appearance.Voice, c.Appearance.VoicePitch, out float residual);
            VoicePlayer.Speak(voice, residual + AgeSemitones(c.Stage), c.Id, emotion, Mathf.Max(8, textLength), VoicePos(c));
        }

        /// <summary>Hazır duygu sesi: pain, laugh, happy, sad, surprised...</summary>
        private void Exclaim(Character c, string state)
        {
            if (c == null || c.IsDead || c.Appearance == null) return;
            string voice = VoicePlayer.VoiceKeyFor((int)c.Appearance.Voice, c.Appearance.VoicePitch, out float residual);
            VoicePlayer.Exclaim(voice, state, residual + AgeSemitones(c.Stage), c.Id, VoicePos(c));
        }

        private void SpeakSocial(Character actor, Character target, SocialAction action, string message)
        {
            var emotion = action == SocialAction.Propose ? VoiceEmotion.Happy : action == SocialAction.TellStory ? VoiceEmotion.Thinking : VoiceEmotion.Neutral;
            Speak(actor, message != null ? message.Length : 20, emotion);
            if (action == SocialAction.Propose) Sfx(SoundKeys.StingWedding);
        }
    }
}
