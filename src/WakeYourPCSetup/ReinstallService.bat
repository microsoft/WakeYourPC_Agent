mkdir C:\WypAgent
copy * C:\WypAgent
sc stop WakeYourPC
sc delete WakeYourPC
sc create WakeYourPC binPath="C:\WypAgentbin\WakeYourPCAgent.exe"
sc start WakeYourPC

