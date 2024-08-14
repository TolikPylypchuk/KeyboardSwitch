source "https://rubygems.org"

gem "jekyll", "~> 3.10"

gem "github-pages", "~> 232", group: :jekyll_plugins

group :jekyll_plugins do
  gem "jekyll-feed", "~> 0.17"
end

install_if -> { RUBY_PLATFORM =~ %r!mingw|mswin|java! } do
  gem "tzinfo", "~> 2.0"
  gem "tzinfo-data"
end

gem "wdm", "~> 0.2.0", :install_if => Gem.win_platform?

gem "kramdown-parser-gfm"
