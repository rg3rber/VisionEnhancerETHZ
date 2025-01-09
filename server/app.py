from flask import Flask, request, jsonify
import logging
import requests
import os
from flask_cors import CORS

app = Flask(__name__)
CORS(app)

# Your Google Cloud Vision API key
GOOGLE_VISION_API_KEY = ""
GOOGLE_VISION_API_URL = "https://vision.googleapis.com/v1/images:annotate"
OPENAI_API_KEY = ""
OPENAI_API_URL = "https://api.openai.com/v1/chat/completions"

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('server.log'),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

def process_with_chatgpt(text):
    headers = {
        "Content-Type": "application/json",
        "Authorization": f"Bearer {OPENAI_API_KEY}"
    }
    
    prompt = """Process this text for speech output:
1. Analyze the input text, do not say 'Analyzing the input text' or anything of the sort
2. If the text contains more than 2 mathematical symbols, equations, or variables, respond with ONLY: 'This appears to be a mathematical formula'
3. Otherwise, remove ALL mathematical content including:
   - Variables (single letters, Greek letters)
   - Numbers with subscripts or superscripts
   - Mathematical operators (+, -, ×, ÷, =, etc.)
   - Any sequences that look like formulas
4. Keep only plain English descriptive text
5. Do not preserve any part of equations or formulas
6. Remove:
   - URLs and email addresses
   - References and citations (e.g., '[1]', 'et al.', 'Figure 3.2')
   - Code snippets or programming syntax
   - Table data and numerical lists
   - Slide numbers or page numbers
   - Lengthy parenthetical asides
   - File paths or technical specifications
7. If the text contains more than 10 instances of the content in point 6, respond with ONLY: 'This content contains technical information not suitable for speech output'"""

    data = {
        "model": "gpt-3.5-turbo",
        "messages": [
            {
                "role": "system",
                "content": "You are a helpful assistant that makes text more suitable for speech output."
            },
            {
                "role": "user",
                "content": f"{prompt}\n\nInput text: {text}"
            }
        ],
        "temperature": 0.7
    }

    try:
        response = requests.post(OPENAI_API_URL, headers=headers, json=data)
        if response.status_code != 200:
            logger.error(f"OpenAI API error: {response.text}")
            return text
        
        processed_text = response.json()['choices'][0]['message']['content']
        return processed_text
    except Exception as e:
        logger.error(f"Error processing with ChatGPT: {str(e)}")
        return text

@app.route('/process_image', methods=['POST'])
def process_image():
    try:
        if not request.is_json:
            raise ValueError("Content-Type must be application/json")

        request_data = request.get_json()
        if not request_data or 'image' not in request_data:
            raise ValueError("Request must contain 'image' field")

        image_data = request_data['image']
        
        vision_request = {
            "requests": [
                {
                    "image": {
                        "content": image_data
                    },
                    "features": [
                        {
                            "type": "TEXT_DETECTION"
                        }
                    ]
                }
            ]
        }

        response = requests.post(
            f"{GOOGLE_VISION_API_URL}?key={GOOGLE_VISION_API_KEY}",
            json=vision_request
        )

        if response.status_code != 200:
            raise Exception(f"Google Vision API error: {response.text}")

        vision_response = response.json()
        
        # Extract text from response
        text_annotations = vision_response['responses'][0].get('textAnnotations', [])
        detected_text = text_annotations[0]['description'] if text_annotations else ""

        # After getting detected_text, process with ChatGPT
        processed_text = process_with_chatgpt(detected_text)

        logger.info("OCR and text processing completed successfully")
        return jsonify({
            'success': True,
            'message': 'Image processed successfully',
            'detected_text': processed_text
        })

    except Exception as e:
        logger.error(f"Error processing image: {str(e)}")
        return jsonify({
            'success': False,
            'error': str(e)
        }), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True) 
