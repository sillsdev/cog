calcpvalue <- function(wordlistSize, seg1Count, seg2Count, seg1Seg2Count)
{
  seg1NotSeg2Count <- seg1Count - seg1Seg2Count
  notSeg1Seg2Count <- seg2Count - seg1Seg2Count
  notSeg1NotSeg2Count = wordlistSize - seg1Seg2Count - notSeg1Seg2Count - seg1NotSeg2Count
  x <- matrix(c(seg1Seg2Count, notSeg1Seg2Count, seg1NotSeg2Count, notSeg1NotSeg2Count), 2)
  p <- chisq.test(x, simulate.p.value = TRUE, B = 100000)$p.value
  return(p / 2)
}

dowork <- function(wordlistSize)
{
  data <- c(0, 0, 0, 0)
  startCount <- 2
  for (seg1Count in 2:round(wordlistSize * 0.3))
  {
    firstCount <- 0
    prevCount <- 1
    for (seg2Count in seg1Count:round(wordlistSize * 0.3))
    {
      found <- FALSE
      for (seg1Seg2Count in max(prevCount, startCount):seg1Count)
      {
        p <- calcpvalue(wordlistSize, seg1Count, seg2Count, seg1Seg2Count)
        if (p < 0.01)
        {
          if (prevCount != seg1Seg2Count)
            data <- rbind(data, c(wordlistSize, seg1Count, seg2Count, seg1Seg2Count))
          if (firstCount == 0)
            firstCount <- seg1Seg2Count
          prevCount = seg1Seg2Count
          found <- TRUE
          break
        }
      }
      if (!found)
        break
      if (prevCount == seg1Count)
        break;
    }
    startCount <- firstCount
  }
  if (dim(data)[1] == 2)
  {
    data <- data[-1,]
    dim(data) <- c(1, 4)
  }
  else
  {
    data <- data[-1,]
  }
  return(data)
}

require("parallel")

filename <- "RegularSoundCorrespondenceThresholdTable.txt"
file.create(filename)
cl <- makeCluster(detectCores())
clusterExport(cl, list("calcpvalue"))
for (data in parLapply(cl, seq(10, 1000, 10), dowork))
  write.table(data, filename, TRUE, sep = "\t", row.names = FALSE, col.names = FALSE)
stopCluster(cl)

system(paste("powershell -file compress.ps1 -i ", filename, " -o RegularSoundCorrespondenceThresholdTable.bin", ""))