mkdir C:\WypAgentbin
copy * C:\WypAgentbin
sc stop WakeYourPC
sc delete WakeYourPC
sc create WakeYourPC binPath="C:\WypAgentbin\WakeYourPCAgent.exe"
sc start WakeYourPC

