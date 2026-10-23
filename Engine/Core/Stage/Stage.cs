using System;

namespace MiMFa.Engine.Stage
{
    public class Stage : StageBase
    {
        public Func<object, Engine, object> Transformer { get; set; }

        public Stage(Func<object, Engine, object> transformer)
        {
            Transformer = transformer;
        }
        public override object Transform(object input, Engine engine)
        {
            Initialize(engine);
            return Transformer(input, engine);
        }
    }
}
