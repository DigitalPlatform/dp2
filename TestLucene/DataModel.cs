using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLucene
{
    public static class DataModel
    {
        static FSDirectory _directory = null;
        static IndexWriter _indexWriter = null;

        public static void Initialize(string data_dir)
        {
            // Ensures index backward compatibility
            const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

            /*
            // Construct a machine-independent path for the index
            var basePath = Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData);
            */
            var indexPath = Path.Combine(data_dir, "index");

            _directory = FSDirectory.Open(indexPath);

            // Create an analyzer to process the text
            var analyzer = new StandardAnalyzer(AppLuceneVersion);

            // Create an index writer
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
            _indexWriter = new IndexWriter(_directory, indexConfig);
        }

        public static void End()
        {
            _indexWriter?.Dispose();
            _directory?.Dispose();
        }

        public static void AddDocument(Document doc)
        {
            _indexWriter.AddDocument(doc);
            _indexWriter.Flush(triggerMerge: false, applyAllDeletes: false);
        }

        public static void DeleteDocument(string id)
        {
            var term = new Term("id", id);
            _indexWriter.DeleteDocuments(term);
        }

        public delegate void delegate_showDoc(ScoreDoc hit, Document doc);

        public static ScoreDoc[] Search(string title,
            delegate_showDoc func_showDoc)
        {
            var query = new TermQuery(new Term("title", title));

            /*
            var phrase = new MultiPhraseQuery
            {
    new Term("title", title),
    new Term("author", author)
};
            */
            using (var reader = _indexWriter.GetReader(applyAllDeletes: true))
            {
                var searcher = new IndexSearcher(reader);
                var hits = searcher.Search(query, 20 /* top 20 */).ScoreDocs;
                
                foreach(var hit in hits)
                {
                    var doc = searcher.Doc(hit.Doc);
                    func_showDoc?.Invoke(hit, doc);
                }

                return hits;

                /*
                // Display the output in a table
                Console.WriteLine($"{"Score",10}" +
                    $" {"Name",-15}" +
                    $" {"Favorite Phrase",-40}");
                foreach (var hit in hits)
                {
                    var foundDoc = searcher.Doc(hit.Doc);
                    Console.WriteLine($"{hit.Score:f8}" +
                        $" {foundDoc.Get("name"),-15}" +
                        $" {foundDoc.Get("favoritePhrase"),-40}");
                }
                */
            }
        }

        // https://stackoverflow.com/questions/2634873/how-do-i-delete-update-a-doc-with-lucene

#if NO
        public static Document NewDocument(string id,
            string title,
            string author)
        {
            var source = new
            {
                Name = "Kermit the Frog",
                FavoritePhrase = "The quick brown fox jumps over the lazy dog"
            };
            var doc = new Document
            {
                new StringField("id", id, Field.Store.YES),
    // StringField indexes but doesn't tokenize
                new StringField("title",
        title,
        Field.Store.YES),
    new TextField("author",
        author,
        Field.Store.YES)
};

            _indexWriter.AddDocument(doc);
            _indexWriter.Flush(triggerMerge: false, applyAllDeletes: false);

            return doc;
        }
#endif
    }
}
