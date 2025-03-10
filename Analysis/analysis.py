import firebase_admin
from firebase_admin import credentials, db
import matplotlib.pyplot as plt
import matplotlib.ticker as mtick

# initialize my own firebase
cred = credentials.Certificate("./csci526playertrack-firebase-adminsdk-fbsvc-b21787ed3b.json")
firebase_admin.initialize_app(cred, {
    'databaseURL': 'https://csci526playertrack-default-rtdb.firebaseio.com/'
})

# fetch my own data
ref = db.reference('/')
data = ref.get()
# print("My current data collected from firebase is: ", data)

# It is time to analyze my data. Before doing so, it is necessary to convert them into the list 
# for further analysis

# results = {}
player_collection = data['players']

user_times_records = {}
for player_comb in player_collection:
    if player_comb is not None:
        currplayTimes = player_comb['totalPlays']

        if currplayTimes not in user_times_records:
            user_times_records[currplayTimes] = 0
        
        user_times_records[currplayTimes] += 1

user_times_records_sorted = sorted(user_times_records.items(), key = lambda x: x[0])
play_times_records = [int(times[0]) for times in user_times_records_sorted]
user_number_records = [int(times[1]) for times in user_times_records_sorted]

# A bar chart can be drawn, where the x-axis represents the number of times played
# and the y-axis represents the exact number of users 
plt.bar(play_times_records, user_number_records, color='green')
plt.title("User Player Frequency Distribution")
plt.xlabel("Player Frequency")
plt.ylabel("User Count")

ax = plt.gca()
ax.xaxis.set_major_locator(mtick.MaxNLocator(integer=True))
ax.yaxis.set_major_locator(mtick.MaxNLocator(integer=True))

plt.show()



    



