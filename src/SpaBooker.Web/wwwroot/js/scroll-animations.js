// Smooth scroll animations for SpaBooker
// Inspired by Maybourne Beverly Hills website

document.addEventListener('DOMContentLoaded', function() {
    // Fade-in animation when sections enter viewport
    const sections = document.querySelectorAll('.content-section');
    
    if (sections.length === 0) return;
    
    const observerOptions = {
        threshold: 0.15, // Trigger when 15% visible
        rootMargin: '0px 0px -50px 0px'
    };
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
            }
        });
    }, observerOptions);
    
    sections.forEach(section => {
        section.classList.add('fade-in-section');
        observer.observe(section);
    });
});
