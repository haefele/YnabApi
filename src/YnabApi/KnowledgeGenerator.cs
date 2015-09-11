namespace YnabApi
{
    public class KnowledgeGenerator
    {
        private int _knowledge;

        internal KnowledgeGenerator(int knowledge)
        {
            this._knowledge = knowledge;
        }

        public int GetNext()
        {
            return ++this._knowledge;
        }

        internal int GetCurrent()
        {
            return this._knowledge;
        }
    }
}