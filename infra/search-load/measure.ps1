param(
    [string] $BaseUrl = 'http://127.0.0.1:5080',
    [int] $Requests = 55
)

$scenarios = [ordered]@{
    'GPS com texto (São Paulo, "eletricista")'        = '/api/v1/providers?lat=-23.55&lng=-46.63&q=eletricista'
    'GPS sem texto (São Paulo)'                       = '/api/v1/providers?lat=-23.55&lng=-46.63'
    'GPS com erro de digitação (Campinas)'            = '/api/v1/providers?lat=-22.90&lng=-47.06&q=eletrisista'
    'GPS, categoria e mais perto (Campinas)'          = '/api/v1/providers?lat=-22.90&lng=-47.06&categoryId=2&sort=distance'
    'Cidade grande sem GPS (São Paulo, "pintor")'     = '/api/v1/providers?cityIbgeCode=3550308&q=pintor'
    'Cidade grande sem GPS e sem filtro (São Paulo)'  = '/api/v1/providers?cityIbgeCode=3550308'
    'Cidade sem GPS, menor preço (Ribeirão Preto)'    = '/api/v1/providers?cityIbgeCode=3543402&sort=price'
    'Início com GPS (São Paulo)'                      = '/api/v1/home?lat=-23.55&lng=-46.63'
}

function Get-Percentile([double[]] $sorted, [double] $percentile) {
    $index = [math]::Ceiling($percentile * $sorted.Count) - 1
    return $sorted[[math]::Max(0, $index)]
}

$client = [System.Net.Http.HttpClient]::new()
$culture = [System.Globalization.CultureInfo]::InvariantCulture
$first = $true
foreach ($name in $scenarios.Keys) {
    if (-not $first) {
        Start-Sleep -Seconds 61
    }

    $first = $false
    $url = "$BaseUrl$($scenarios[$name])"
    $client.GetAsync($url).GetAwaiter().GetResult().Dispose()
    $timings = for ($attempt = 1; $attempt -lt $Requests; $attempt++) {
        $watch = [System.Diagnostics.Stopwatch]::StartNew()
        $response = $client.GetAsync($url).GetAwaiter().GetResult()
        $null = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        $watch.Stop()
        if (-not $response.IsSuccessStatusCode) {
            throw "$name respondeu $([int]$response.StatusCode)"
        }

        $response.Dispose()
        $watch.Elapsed.TotalMilliseconds
    }

    $sorted = [double[]] ($timings | Sort-Object)
    [string]::Format($culture, '{0,-50} p50 {1,5:F0} ms   p95 {2,5:F0} ms   ({3} requisições)', $name, (Get-Percentile $sorted 0.5), (Get-Percentile $sorted 0.95), $sorted.Count)
}
