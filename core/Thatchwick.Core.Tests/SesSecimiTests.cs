using Thatchwick.Core.Audio;
using Thatchwick.Core.World;
using Xunit;

namespace Thatchwick.Core.Tests
{
    public class SesSecimiTests
    {
        [Fact]
        public void Adim_ZemineVeHavayaGore()
        {
            Assert.Equal(SoundKeys.StepMine, SoundPicker.Step(true, FloorType.Wood, RoadType.None, "farm", 0, false));
            Assert.Equal(SoundKeys.StepWood, SoundPicker.Step(false, FloorType.Wood, RoadType.None, "farm", 1, false));   // içeride yağmur çamur yapmaz
            Assert.Equal(SoundKeys.StepStone, SoundPicker.Step(false, FloorType.Grass, RoadType.Paved, "town", 0, false));
            Assert.Equal(SoundKeys.StepMud, SoundPicker.Step(false, FloorType.Grass, RoadType.Path, "road", 1, false));
            Assert.Equal(SoundKeys.StepSnow, SoundPicker.Step(false, FloorType.Grass, RoadType.None, "farm", 0, true));
            Assert.Equal(SoundKeys.StepSand, SoundPicker.Step(false, FloorType.Grass, RoadType.None, "coast", 0, false));
            Assert.Equal(SoundKeys.StepDirt, SoundPicker.Step(false, FloorType.Grass, RoadType.Path, "road", 0, false));
            Assert.Equal(SoundKeys.StepGrass, SoundPicker.Step(false, FloorType.Grass, RoadType.None, "farm", 0, false));
        }

        [Fact]
        public void Is_Hayvan_Bildirim()
        {
            Assert.Equal(SoundKeys.WorkChop, SoundPicker.Work("chop", "chop"));
            Assert.Equal(SoundKeys.WorkWater, SoundPicker.Work("farm", "water"));
            Assert.Equal(SoundKeys.WorkHoe, SoundPicker.Work("farm", "till"));
            Assert.Equal(SoundKeys.WorkCraft, SoundPicker.Work("craft", "craft:stool"));
            Assert.Null(SoundPicker.Work("eat", "eat_egg"));
            Assert.Equal(SoundKeys.AnimalRooster, SoundPicker.Animal("animal.chicken", adultMale: true));
            Assert.Equal(SoundKeys.AnimalHorse, SoundPicker.Animal("animal.mule"));
            Assert.Null(SoundPicker.Animal("animal.bees"));
            Assert.Equal(SoundKeys.UiNotifyDanger, SoundPicker.Notice(2));
            Assert.Equal(SoundKeys.UiSell, SoundPicker.Build("sell"));
        }
    }
}
