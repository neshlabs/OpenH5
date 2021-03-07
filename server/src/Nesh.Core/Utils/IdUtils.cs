using IdGen;

namespace Nesh.Core.Utils
{
    public static class IdUtils
    {
        static IdUtils()
        {
            UGen = new IdGenerator(0, new IdGeneratorOptions(new IdStructure(45, 2, 16), new DefaultTimeSource(TimeUtils.Now)));
            RGen = new IdGenerator(1, new IdGeneratorOptions(new IdStructure(45, 2, 16), new DefaultTimeSource(TimeUtils.Now)));
        }

        public static IdGenerator UGen { get; private set; }

        public static IdGenerator RGen { get; private set; }
    }
}
