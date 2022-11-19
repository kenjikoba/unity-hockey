#%%
import matplotlib.pyplot as plt
import pandas as pd
import numpy as np



#%%
q_file = '20221118164202.csv'
data = pd.read_csv(q_file)
data.head()
#%%
data.describe()
# x = np.array([1,2,3,4])
# y = np.array([2,3,4,5])

# # グラフを描画
# # %%
# plt.plot(x, y)
# plt.show()
# %%
gamecount = data[data.keys()[0]]
episode = data[data.keys()[1]]
average_max_q = data[data.keys()[2]]
score = data[data.keys()[3]]
average_diff = data[data.keys()[4]]
gamecount = np.array(gamecount)
episode = np.array(episode)
average_max_q = np.array(average_max_q)
score = np.array(score)
average_diff = np.array(average_diff)
plt.title('Gamecount-Episode graph')
plt.xlabel('Gamecount')
plt.ylabel('Episodes')
plt.plot(gamecount, episode, marker='')
plt.savefig("Gamecount-Episode.png")
plt.show()
# %%
plt.title('Gamecount-Score graph')
plt.xlabel('Gamecount')
plt.ylabel('Score')
plt.plot(gamecount, score, marker='')
plt.savefig("Gamecount-Score.png")
plt.show()
# %%
plt.title('Gamecount-MaxQ graph')
plt.xlabel('Gamecount')
plt.ylabel('Average-MaxQ')
plt.plot(gamecount, average_max_q, marker='')
plt.savefig("Gamecount-Average-MaxQ.png")
plt.show()
# %%
plt.title('Gamecount-Diff graph')
plt.xlabel('Gamecount')
plt.ylabel('Average-diff')
plt.plot(gamecount, average_diff, marker='')
plt.savefig("Gamecount-Average-diff.png")
plt.show()
# %%
