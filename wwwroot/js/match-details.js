// Match Details JavaScript for live commentary loading with SSE (used Claude to help me write this code)

document.addEventListener('DOMContentLoaded', function() {
    // Get match details from data attributes
    const matchDetailsContainer = document.getElementById('matchDetailsData');
    if (!matchDetailsContainer) {
        console.log('No match details container found - not in loading mode');
        return;
    }
    
    const matchId = matchDetailsContainer.dataset.matchId;
    const mode = matchDetailsContainer.dataset.mode;
    const streamUrl = matchDetailsContainer.dataset.streamUrl;
    
    console.log('Starting SSE connection:', { matchId, mode, streamUrl });
    
    const loadingState = document.getElementById('loadingState');
    const eventsList = document.getElementById('eventsList');
    const homeScoreEl = document.getElementById('homeScore');
    const awayScoreEl = document.getElementById('awayScore');
    
    // Event queue system
    let eventQueue = [];
    let isProcessingQueue = false;
    
    // Connect to SSE endpoint
    const eventSource = new EventSource(streamUrl);
    
    function displayEvent(event) {
        console.log('Displaying event:', event.minute, event.type);
        // Hide loading state and show events list on first event
        if (loadingState && loadingState.style.display !== 'none') {
            loadingState.style.display = 'none';
            eventsList.style.display = 'block';
        }
        
        // Create event element
        const eventItem = document.createElement('div');
        eventItem.className = 'event-item ' + event.type.toLowerCase();
        
        const eventHeader = document.createElement('div');
        eventHeader.className = 'event-header';
        
        const eventTime = document.createElement('span');
        eventTime.className = 'event-time';
        eventTime.textContent = event.minute + "'";
        
        const eventType = document.createElement('span');
        eventType.className = 'event-type';
        eventType.textContent = event.type;
        
        eventHeader.appendChild(eventTime);
        eventHeader.appendChild(eventType);
        
        const eventDescription = document.createElement('div');
        eventDescription.className = 'event-description';
        eventDescription.textContent = event.description;
        
        eventItem.appendChild(eventHeader);
        eventItem.appendChild(eventDescription);
        
        // Add score for goals
        if (event.type === 'Goal') {
            const eventScore = document.createElement('div');
            eventScore.className = 'event-score';
            eventScore.textContent = 'Score: ' + event.homeScore + ' - ' + event.awayScore;
            eventItem.appendChild(eventScore);
            
            // Update header scores with animation
            homeScoreEl.textContent = event.homeScore;
            awayScoreEl.textContent = event.awayScore;
            
            // Add pulse animation
            homeScoreEl.style.animation = 'none';
            awayScoreEl.style.animation = 'none';
            setTimeout(() => {
                homeScoreEl.style.animation = 'pulse 0.5s';
                awayScoreEl.style.animation = 'pulse 0.5s';
            }, 10);
        }
        
        // Append event to list with fade-in
        eventItem.style.opacity = '0';
        eventItem.style.transform = 'translateY(-10px)';
        eventsList.appendChild(eventItem);
        
        // Trigger animation
        setTimeout(() => {
            eventItem.style.transition = 'opacity 0.3s, transform 0.3s';
            eventItem.style.opacity = '1';
            eventItem.style.transform = 'translateY(0)';
        }, 10);
        
        // Smooth scroll to bottom
        eventsList.scrollTo({
            top: eventsList.scrollHeight,
            behavior: 'smooth'
        });
    }
    
    // Process event queue with 1-second delays
    async function processQueue() {
        if (isProcessingQueue) {
            console.log('Already processing queue, skipping...');
            return;
        }
        
        console.log('Queue processor started');
        isProcessingQueue = true;
        
        while (true) {
            // Wait for events to arrive or process existing ones
            if (eventQueue.length === 0) {
                // Check if SSE is still connected
                if (eventSource.readyState === EventSource.CLOSED) {
                    console.log('SSE closed and queue empty, stopping processor');
                    break;
                }

                await new Promise(resolve => setTimeout(resolve, 100));
                continue;
            }
            
            const event = eventQueue.shift();
            console.log('Processing event:', event.minute, event.type, '- Queue remaining:', eventQueue.length);
            displayEvent(event);
            
            // Wait 1 second before displaying next event 
            console.log('Waiting 1 second before next event...');
            await new Promise(resolve => setTimeout(resolve, 1000));
        }
        
        console.log('Queue processing complete');
        isProcessingQueue = false;
    }
    
    // Handle commentary events
    eventSource.addEventListener('commentary', function(e) {
        try {
            const event = JSON.parse(e.data);
            console.log('Received event from SSE:', event.minute, event.type);
            eventQueue.push(event);
            console.log('Event added to queue. Total in queue:', eventQueue.length);
            
            // Only start processor if not already running
            if (!isProcessingQueue) {
                processQueue();
            }
        } catch (error) {
            console.error('Error parsing commentary event:', error);
        }
    });
    
    // Handle match completion
    eventSource.addEventListener('matchComplete', function(e) {
        try {
            const result = JSON.parse(e.data);
            console.log('Match completed:', result);
            
            // Close event source
            eventSource.close();
            
        } catch (error) {
            console.error('Error parsing match complete event:', error);
        }
    });
    
    // Handle errors
    eventSource.addEventListener('error', function(e) {
        console.error('SSE error:', e);
        
        if (e.data) {
            try {
                const error = JSON.parse(e.data);
                alert('Error: ' + error.error);
            } catch (parseError) {
                console.error('Error parsing error message:', parseError);
            }
        }
        
        eventSource.close();
        
        // Show error message
        if (loadingState) {
            loadingState.innerHTML = '<div class="alert alert-danger">An error occurred while loading match commentary. Please try again.</div>';
        }
    });
    
    // Cleanup on page unload
    window.addEventListener('beforeunload', function() {
        eventSource.close();
    });
    
});
