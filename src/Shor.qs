namespace ShorQuantum
{
    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Primitive;
    open Microsoft.Quantum.Extensions.Math;
    open Microsoft.Quantum.Extensions.Convert;
    open Microsoft.Quantum.Extensions.Diagnostics;

    operation QFT (qubits: Qubit[], len: Int) : ()
    {
        body
        {
            let fi = 2.0*PI();

            for (i in 0..len-1){
                H(qubits[i]);
                for (j in 0..len-1)
                {
                    (Controlled R1)([qubits[j+1]],(fi/PowD(2.0, ToDouble(j+1)),  qubits[i]));
                }
            }

            for (i in 0..len/2-1){
                SWAP(qubits[i],qubits[len-1-i]);
            }
        }
    }

    operation IQFT(qubits: Qubit[], len: Int):()
    {
        body{
            let fi = -2.0*PI();

            for (i in 0..len/2-1){
                SWAP(qubits[i],qubits[len-1-i]);
            }

            for (i in 0..len-1){
                for (j in i..-1..1){
                    (Controlled R1)([qubits[i-j]],(fi/PowD(2.0, ToDouble(j)),  qubits[i]));
                }
                H(qubits[i]);
            }
        }
    }

    operation Ua(qubits: Qubit[], x: Int, N: Int):(){
        body{
            let le = LittleEndian(qubits);
            ModularMultiplyByConstantLE(x, N, le);
        }
        controlled auto;
    }

    function power2(x : Int):(Int){
            mutable result = 1;
            for (i in 0..x-1){
                set result=result*2;
            }
            return result;
    }

    function modexp(x: Int, scpt: Int, N: Int) : (Int) {
        mutable scp = scpt;
        mutable vx = x;
        mutable result = 1;
        for (i in 0..15){
            if (scp%2==1){
                set result = result * vx % N;
            }
            set vx = vx * vx % N;
            set scp = scp / 2;
        }
        
        return result;
    }

    operation QuantumOrderFinding(x: Int, N: Int, a: Int, b: Int):(Int[]){
        body{
            let l = a;
            let t = b;
            mutable results = new Int[t];
            mutable ep = 1;
            using(qubits = Qubit[l+t]){
                let Reg1 = qubits[0..t-1];
                let Reg2 = qubits[t..l+t-1];
                
                // Reg2 init to |1>
                X(Reg2[0]);
                // apply H to Reg1
                for (i in 0..t-1){
                    H(qubits[i]);
                }
                
                // apply controlled ux
                for (i in 0..t-1){
                    set ep = power2(t-1-i);
                    (Controlled Ua)([qubits[i]],(Reg2, modexp(x,ep,N), N));
                }

                // apply inverse QFT
                IQFT(Reg1, t);

                // measure Reg1
                for (i in 0..t-1){
                    if(M(qubits[i]) == Zero){
                        set results[i] = 0;
                    }
                    else{
                        set results[i] = 1;
                    }
                }
                ResetAll(qubits);
            }
            return results;
        }
    }

    // unit test for QFT
    operation TestQFT():(Int[]){
        body{
            mutable results = new Int[4];
            using(qs=Qubit[4]){
                QFT(qs, 4);
                for (i in 0..3){
                    if(M(qubits[i]) == Zero){
                        set results[i] = 0;
                    }
                    else{
                        set results[i] = 1;
                    }
                }
                ResetAll(qs);
            }
            return results;
        }
    }

    // unit test for IQFT
    operation TestIQFT():(Int[]){
        body{
            mutable results = new Int[t];
            using(qs=Qubit[4]){
                IQFT(qs, 4);
                for (i in 0..t-1){
                    if(M(qubits[i]) == Zero){
                        set results[i] = 0;
                    }
                    else{
                        set results[i] = 1;
                    }
                }
                ResetAll(qs);
            }
            return results;
        }
    }

    // unit test for Ua
    operation TestUa() : (Int[]) {
        body{
            mutable results = new Int[t];
            using(qs=Qubit[5]){
                X(qs[2]);
                X(qs[3]);
                Ux(qs,11,21);
                for (i in 0..4){
                    if(M(qubits[i]) == Zero){
                        set results[i] = 0;
                    }
                    else{
                        set results[i] = 1;
                    }
                }
                ResetAll(qs);
            }
            return results;
        }
    }
    
    // unit test for QuantumOrderFinding
    operation TestOrderFinding() : (Int[]) {
        body{
            return QuantumOrderFinding(3,21,5,6);   
        }
    }
    

}
