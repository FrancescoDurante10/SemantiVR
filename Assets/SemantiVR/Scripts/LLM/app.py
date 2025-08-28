from fastapi import FastAPI
from fastapi import Query
from pydantic import BaseModel
import gensim.models
import numpy as np

app = FastAPI()

print("Loading Word2Vec model from local file...")
word2vec_model = gensim.models.KeyedVectors.load("word2vec-google-news-300-downloaded.model")
print("Model loaded.")

class DescriptionPair(BaseModel):
    description1: str
    description2: str

def cosine_similarity(vec1, vec2):
    if np.linalg.norm(vec1) == 0 or np.linalg.norm(vec2) == 0:
        return 0.0
    return np.dot(vec1, vec2) / (np.linalg.norm(vec1) * np.linalg.norm(vec2))

def get_average_vector(description):
    words = description.lower().split()
    word_vectors = []

    for word in words:
        if word in word2vec_model:
            word_vectors.append(word2vec_model[word])

    if len(word_vectors) == 0:
        return np.zeros(300)

    return np.mean(word_vectors, axis=0)

@app.post("/semantic-compatibility")
async def calculate_semantic_compatibility(pair: DescriptionPair):
    description1 = pair.description1
    description2 = pair.description2

    vec1 = get_average_vector(description1)
    vec2 = get_average_vector(description2)

    similarity = cosine_similarity(vec1, vec2)
    return {"compatibility_score": float(similarity)}  

@app.get("/check-word")
async def check_word(word: str = Query(...)):
    words = word.lower().split()
    found = [w for w in words if w in word2vec_model]
    missing = [w for w in words if w not in word2vec_model]

    return {
        "input": word,
        "found_words": found,
        "missing_words": missing,
        "all_found": len(missing) == 0
    }
