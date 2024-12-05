from app import create_app, db
from app.models.user import User
from app.models.restaurant import Restaurant
import requests
import time
from datetime import datetime
from flask_login import login_required, current_user
import json
from app.models.sensor_data import SensorData
import threading
app = create_app()

def init_db():
   with app.app_context():
       db.create_all()
       
       # 샘플 레스토랑 데이터 생성
       restaurants_data = [
           {
               'id': 'id-mn',
               'name': '맛있는 한식당',
               'description': '전통 한식을 현대적으로 재해석한 맛집. 계절별 특선 메뉴와 함께 정갈한 한상차림을 즐기실 수 있습니다.',
               'image_path': 'images/korean-restaurant.jpg',
               'rating': 4.5,
               'address': '서울시 강남구 역삼동 123-45',
               'latitude': 37.5012,
               'longitude': 127.0396,
               'temperature': 23.5,
               'humidity': 45.0,
               'noise_level': 65.0,
               'short_reviews': [
                   "깔끔한 한식을 즐길 수 있는 곳입니다."
               ]
           },
           {
               'id': 'rest002', 
               'name': '이탈리안 키친',
               'description': '정통 이탈리안 요리를 즐길 수 있는 분위기 좋은 레스토랑. 쉐프가 직접 엄선한 와인과 페어링도 가능합니다.',
               'image_path': 'images/italian-restaurant.jpg',
               'rating': 4.8,
               'address': '서울시 서초구 서초동 456-78',
               'latitude': 37.4923,
               'longitude': 127.0292,
               'temperature': 22.0,
               'humidity': 40.0,
               'noise_level': 58.0,
               'short_reviews': [
                   "파스타가 정말 맛있어요!"
               ]
           },
           {
               'id': 'rest003',
               'name': '스시 하우스',
               'description': '신선한 해산물로 만드는 정통 일식 스시. 제철 생선으로 만드는 특선 스시를 맛보실 수 있습니다.',
               'image_path': 'images/sushi-restaurant.jpg',
               'rating': 4.7,
               'address': '서울시 강남구 청담동 789-10',
               'latitude': 37.5248,
               'longitude': 127.0489,
               'temperature': 21.5,
               'humidity': 42.0,
               'noise_level': 52.0,
               'short_reviews': [
                   "신선한 스시를 즐길 수 있어요"
               ]
           }
       ]
       
       # 레스토랑과 관리자 계정 생성
       for rest_data in restaurants_data:
           restaurant_id = rest_data['id']
           
           # 레스토랑이 없으면 생성
           if not Restaurant.query.get(restaurant_id):
               restaurant = Restaurant(**rest_data)
               db.session.add(restaurant)               
               # 관리자 계정 생성 (레스토랑 ID를 사용자 ID로 사용)
               if not User.query.get(restaurant_id):
                   user = User.create_user(restaurant_id, "password123")  # 기본 비밀번호 설정
       
       db.session.commit()

def send_get_request(url: str = "", originator: str = "CAdmin"):
    headers = {
        "Accept": "application/json",
        "X-M2M-RI": "12345",
        "X-M2M-Origin": originator,
        "X-M2M-RVI": "3"
    }
    
    try:        
        response = requests.get(url, headers=headers, verify=False)                
        return response
        
    except requests.exceptions.RequestException as e:
        print(f"Error occurred: {e}")
        return None

def create_headers(originator: str, resource_type: str = None, request_id: str = None, time: str = None, rsc: str = None) -> dict:
    headers = {
        "Accept": "application/json",
        "X-M2M-Origin": originator,
        "X-M2M-RVI": "3"        
    }

    if rsc:
        headers["X-M2M-RSC"] = rsc

    if resource_type:
        headers["Content-Type"] = 'application/json;ty=' + resource_type

    if request_id:
        headers["X-M2M-RI"] = request_id

    if time:
        headers["'X-M2M-OT'"] = time

    return headers

def create_content_instance(container_url, data, originator):
    # Create headers with resource type 4 for content instance
    # The resource type 4 is specifically for content instances in oneM2M
    header = create_headers(originator, '4', '12345')
    content_instance_payload = {
        "m2m:cin": {
            "con": data  # The actual data/content to be stored
        }
    }
    try:
        # Send POST request to create the content instance
        response = requests.post(
            container_url, 
            headers=header, 
            json=content_instance_payload, 
            verify=False
        )
        # Check if creation was successful (201 Created)
        if response.status_code == 201:
            return True
        else:
            print(f"Failed to create Content Instance: {response.status_code} {response.text}")
            return False
            
    except requests.exceptions.RequestException as e:
        print(f"Error creating Content Instance: {e}")
        return False

def sync_data():    
    time.sleep(5)    
    with app.app_context():
        while True:                    
            csr_list = send_get_request("http://localhost:8080/id-in?rcn=4", "CAdmin").json()["m2m:cb"]["m2m:csr"]
            print(len(csr_list))
            for csr in csr_list:
                csr_name = csr["rn"]            
                url = f"http://localhost:8080/~/{csr_name}/cse-mn/Sensor/sensor_grp/fopt/la"                
                print(url)
                res = send_get_request(url, "CAdmin").json()["m2m:agr"]["m2m:rsp"]
                total = {}
                for data in res:
                    total.update(json.loads(data['pc']['m2m:tsi']['con']))
                sensor_data = SensorData(
                    restaurant_id=csr_name,
                    temperature=total["temp"],
                    humidity=total["humidity"],
                    noise_level=total["noise"],
                    timestamp=datetime.now()
                )    
                print(datetime.now())
                db.session.add(sensor_data)
                User.query.get(csr_name).restaurant.temperature = total["temp"]
                User.query.get(csr_name).restaurant.humidity = total['humidity']
                User.query.get(csr_name).restaurant.noise_level = total['noise']
                db.session.commit()
            time.sleep(10) #time span 60sec
    

if __name__ == '__main__':    
    init_db()  # 서버 시작 시 데이터베이스 초기화
    threading.Thread(target=sync_data).start()
    app.run(debug=False, host= '0.0.0.0', port = '18080')
