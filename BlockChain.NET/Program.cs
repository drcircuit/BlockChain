using System;
using static System.Text.Encoding;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockChain.NET
{

        //Now we have a rudimentary blockchain.... but this can be spammed.. we can't have that!!! we need a Proof Of Work... 
        // that just means somebody needs to prove they tried really really hard to make a new block...
        class Block{
        public Block(string data, byte[] previousHash, int difficulty)
        {
            this.Data = data;
            this.PreviousHash = previousHash  ?? new byte[2];
            this.TimeStamp = DateTime.UtcNow;
            
            this.Difficulty = difficulty;
            this.DifficultyString = GetDiffString(difficulty);
            Console.WriteLine($"Difficulty: {Difficulty} : {DifficultyString}");
            this.Nonce = 0;
            this.Hash = CalculateHash(); 
        }

        private string GetDiffString(int difficulty)
        {
            StringBuilder sb = new StringBuilder();
            for(var i = 0;i<difficulty;i++){
                sb.Append("0");
            }
            return sb.ToString();
        }

        public byte[] CalculateHash(){
            using(var alg = SHA256.Create()){
                byte[] hash =null;
                while(true)
                {
                    hash = alg.ComputeHash(ASCII.GetBytes(Data+TimeStamp.ToString()+Nonce.ToString())); 
                    if(!MatchesBlockChainCriteria(hash)){
                        Nonce++;
                    } else {
                        break;
                    }
                }   
                Console.WriteLine($"Mined a block!!!!: {HashToString(hash)}");
                return hash;
            }
        }

        private bool MatchesBlockChainCriteria(byte[] hash)
        {
            if(hash == null)
                return false;
            return HashToString(hash).Substring(0, Difficulty) == DifficultyString;
        }

        public string Data { get; set; } // just to prove a point... :)
        public byte[] PreviousHash { get; set; }
        public byte[] Hash { get; }

        private int Difficulty;
        private string DifficultyString;
        private int Nonce;

        public DateTime TimeStamp{get;}
        public string HashString => HashToString(Hash);
        public string PrevHashString => HashToString(PreviousHash);
        public string HashToString(byte[] hash){
            return BitConverter.ToString(hash).Replace("-","");
        }

        internal bool ChecksOut() => HashToString(CalculateHash()) == HashToString(Hash);

        internal bool LinksTo(Block previous)
        {
            return HashToString(PreviousHash) == HashToString(previous.Hash);
        }
    }
    class BlockChain{
        public BlockChain(int difficulty)
        {
            this.Difficulty = difficulty;
            Blocks = new List<Block>();
            Blocks.Add(new Block("Genesis", null, Difficulty));
        }
        public void Add(Block block){
            var lastBlock = Blocks.Last();
            block.PreviousHash = lastBlock.Hash;
            Blocks.Add(block);
        }

        public int Difficulty { get; }
        public List<Block> Blocks { get; private set; }
        public override string ToString(){
            StringBuilder sb = new StringBuilder();
            foreach(var b in Blocks){
                sb.AppendLine($"{b.TimeStamp:YYYY-MM-DD-HH:MM:SS} : {b.Data} : {b.HashString} : {b.PrevHashString}");
            }
            return sb.ToString();                        
        }

        public bool IsValid(){
            var pi = 0;
            for(var i = 1; i < Blocks.Count; i++){
                var block = Blocks[i];
                if(!block.ChecksOut()){
                    return false;
                }
                var previous = Blocks[pi++];
                if(!block.LinksTo(previous)){ 
                    return false;
                }                
            }

            return true;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello BlockChain!");
            var eCoin = new BlockChain(4);
            eCoin.Add(new Block("10 monnies", null, eCoin.Difficulty));
            eCoin.Add(new Block("20 monnies", null, eCoin.Difficulty));
            eCoin.Add(new Block("3 monnies", null, eCoin.Difficulty));
            eCoin.Add(new Block("2 monnies", null, eCoin.Difficulty));
            eCoin.Add(new Block("7 monnies", null, eCoin.Difficulty));
            Console.WriteLine(eCoin.ToString());
            Console.WriteLine($"My chain for sure checks out!:{eCoin.IsValid()}");
            eCoin.Blocks[3] = eCoin.Blocks.ElementAt(2);
            Console.WriteLine($"My chain for sure checks out!:{eCoin.IsValid()}"); // This should be false...
            eCoin.Blocks.ElementAt(3).Data = "20000 monnies!!!!!"; 
            Console.WriteLine($"My chain for sure checks out!:{eCoin.IsValid()}"); // This should be false...
        }
    }
}
