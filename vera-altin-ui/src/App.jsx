import { useState, useEffect } from 'react'
import * as signalR from '@microsoft/signalr'
import './App.css'

function App() {
  const [usdPrice, setUsdPrice] = useState(null)
  const [tlPrice, setTlPrice] = useState(null)
  const [usdTry, setUsdTry] = useState(34.50)
  const [lastUpdate, setLastUpdate] = useState('Bağlantı bekleniyor...')
  const [pulse, setPulse] = useState(false)

  // Dolar kurunu çek
  async function getExchangeRate() {
    try {
      const res = await fetch('https://open.er-api.com/v6/latest/USD')
      const data = await res.json()
      setUsdTry(data.rates.TRY)
    } catch (err) {
      console.error('Kur çekilemedi:', err)
    }
  }

  useEffect(() => {
    getExchangeRate()
    const interval = setInterval(getExchangeRate, 300000) // 5 dakikada bir

    // SignalR bağlantısı
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5184/goldHub')
      .withAutomaticReconnect()
      .build()

    connection.on('ReceiveGoldPrice', (goldData) => {
      const price = goldData.price || goldData.Price
      const gram = (price / 31.10347) * usdTry

      setUsdPrice(price)
      setTlPrice(gram)
      setLastUpdate('Son Güncelleme: ' + new Date().toLocaleTimeString())

      // pulse efekti
      setPulse(false)
      setTimeout(() => setPulse(true), 10)
    })

    async function start() {
      try {
        await connection.start()
        console.log('SignalR bağlandı!')
        setLastUpdate('Veri bekleniyor...')
      } catch (err) {
        console.log('Bağlantı hatası:', err)
        setTimeout(start, 5000)
      }
    }

    start()

    return () => {
      clearInterval(interval)
      connection.stop()
    }
  }, [])

  // usdTry değişince tlPrice'ı yeniden hesapla
  useEffect(() => {
    if (usdPrice !== null) {
      const gram = (usdPrice / 31.10347) * usdTry
      setTlPrice(gram)
    }
  }, [usdTry])

  return (
    <div className="wrapper">
      <div className="card">
        <h2>Vera Altın Takip</h2>

        <div className="price-section">
          <small>ONS FİYATI (USD)</small>
          <div id="price">
            {usdPrice !== null
              ? usdPrice.toLocaleString('en-US', { style: 'currency', currency: 'USD' })
              : '$0,00'}
          </div>
        </div>

        <div className="exchange-rate">
          <small>GÜNCEL KUR (USD/TRY)</small>
          <div id="usd-try">
            {usdTry
              ? '1 $ = ' + usdTry.toLocaleString('tr-TR', { minimumFractionDigits: 4 }) + ' ₺'
              : 'Yükleniyor...'}
          </div>
        </div>

        <hr />

        <div className="price-section">
          <small>GRAM ALTIN (TL)</small>
          <div id="tl-price" className={pulse ? 'pulse' : ''}>
            {tlPrice !== null
              ? tlPrice.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })
              : '₺0,00'}
          </div>
        </div>

        <div className="update-time">{lastUpdate}</div>
      </div>
    </div>
  )
}

export default App
