import gensim.downloader as api

print("Downloading Word2Vec...")
model = api.load("word2vec-google-news-300")
model.save("word2vec-google-news-300-downloaded.model")
print("Model saved locally!")
