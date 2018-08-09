import getpass, time, cmath
from qiskit import ClassicalRegister, QuantumRegister, QuantumCircuit
from qiskit import available_backends, execute, register, least_busy
from qiskit.tools.visualization import plot_histogram, circuit_drawer

APItoken = 'ef673f8f495d4c17f466c4daa50ea09e261aaa80bafaa13989f1d18a796efc757fdd9bdc076fd0ee6d0db664f8d0ec9b7e3a53f8eceaa41c2441762bc97f109d'
qx_config = {
    "APItoken": APItoken,
    "url": "https://quantumexperience.ng.bluemix.net/api"
}
try:
    register(qx_config['APItoken'], qx_config['url'])
except:
    print("Something went wrong!")

backend = least_busy(available_backends({'simulator': False, 'local':False})) 
cpi = cmath.pi

def qswap(q:QuantumRegister, qc:QuantumCircuit, num1:int, num2:int):
    qc.cx(q[num1],q[num2])
    qc.cx(q[num2],q[num1])
    qc.cx(q[num1],q[num2])
    return qc

def qft(q:QuantumRegister, qc:QuantumCircuit, len:int):
    for i in range(0,len):
        qc.h(q[i])
        for j in range(i+1,len):
            qc.cu1(cpi/pow(2,j-i), q[j], q[i])
    for i in range(0, int(len/2)):
        qc = qswap(q,qc,i,len-i-1)
          
    return qc

def rqft(q:QuantumRegister, qc:QuantumCircuit, len:int):
    for i in range(0, int(len/2)):
        qc = qswap(q,qc,i,len-i-1)
    for i in range(0,len):
        for j in range(1,i+1)[::-1]:
            qc.cu1(-cpi/pow(2,j), q[i-j], q[i])
        qc.h(q[i])
    return qc

def add(q:QuantumRegister, qc:QuantumCircuit, l:int, t:int):
    for i in range(0,l):
        for j in range(0,l-i):
            qc.cu1(cpi/pow(2,j), q[i+j+t+l], q[i+t])

l = 5
t = 6   
q = QuantumRegister(l+t)
c = ClassicalRegister(t)
qc = QuantumCircuit(q,c)

for i in range(t, l+t):
    qc.x(q[i])
qc.barrier()

for i in range(0,t):
    qc.h(q[i])

qc = rqft(q,qc,t)
qc.barrier()

for i in range(0,t):
    qc.measure(q[i],c[i])

job_exp = execute(qc, backend=backend, shots=1024, max_credits=3)

lapse = 0
interval = 30
while not job_exp.done:
    print('Status @ {} seconds'.format(interval*lapse))
    print(job_exp.status)
    time.sleep(interval)
    lapse += 1
print(job_exp.status)
plot_histogram(job_exp.result().get_counts(qc))
circuit_drawer(qc).show()